using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace PlayerSpeed;

[ApiVersion(2, 1)]
public class PlayerSpeed : TerrariaPlugin
{
    #region 插件信息
    public override string Name => "玩家速度";
    public override string Author => "逆光奔跑 羽学";
    public override Version Version => new Version(1, 2, 1);
    public override string Description => "使用指令设置玩家移动速度 并在冲刺或穿上自定义装备跳跃时触发";
    #endregion

    #region 全局变量
    internal static Configuration Config = new();
    public static Database DB = new();
    #endregion

    #region 注册与释放
    public PlayerSpeed(Main game) : base(game) { }
    public override void Initialize()
    {
        LoadConfig();
        GeneralHooks.ReloadEvent += ReloadConfig;
        GetDataHandlers.PlayerUpdate.Register(this.OnPlayerUpdate);
        ServerApi.Hooks.NetGreetPlayer.Register(this, this.OnGreetPlayer);
        TShockAPI.Commands.ChatCommands.Add(new Command("vel.admin", Commands.vel, "vel", "速度"));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= ReloadConfig;
            GetDataHandlers.PlayerUpdate.UnRegister(this.OnPlayerUpdate);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, this.OnGreetPlayer);
            TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == Commands.vel);
        }
        base.Dispose(disposing);
    }
    #endregion

    #region 配置重载读取与写入方法
    private static void ReloadConfig(ReloadEventArgs args = null!)
    {
        LoadConfig();
        args.Player.SendInfoMessage("[玩家速度]重新加载配置完毕。");
    }

    private static void LoadConfig()
    {
        Config = Configuration.Read();
        Config.Write();
    }
    #endregion

    #region 创建玩家数据方法
    private void OnGreetPlayer(GreetPlayerEventArgs args)
    {
        if (args == null || !Config.Enabled)
        {
            return;
        }

        var plr = TShock.Players[args.Who];

        if (plr == null)
        {
            return;
        }

        var data = DB.GetData(plr.Name);

        // 如果玩家不在数据表中，则创建新数据
        if (data == null)
        {
            var newData = new Database.PlayerData
            {
                Name = plr.Name,
                Enabled = false,
                Count = 0,
                CoolTime = DateTime.UtcNow,
            };
            DB.AddData(newData); // 添加到数据库
        }
    }
    #endregion

    #region 玩家加速冲刺方法
    private void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs e)
    {
        var plr = e.Player;
        var tplr = plr.TPlayer;
        var data = DB.GetData(plr.Name);
        if (plr == null || data == null || !Config.Enabled || !plr.IsLoggedIn || !plr.Active || !plr.HasPermission("vel.use")) return;
        var now = DateTime.UtcNow;
        var LastCool = data.CoolTime != default ? (float)Math.Round((now - data.CoolTime).TotalSeconds, 2) : 0f;
        var CRight = tplr.controlRight && tplr.direction == 1;
        var CLeft = tplr.controlLeft && tplr.direction == -1;
        var armor = tplr.armor.Take(20).Any(x => Config.ArmorItem != null && Config.ArmorItem.Contains(x.netID));

        //计算冷却过去多久
        if (!data.Enabled && LastCool >= Config.CoolTime)
        {
            data.Count = 0;
            data.Enabled = true;
            DB.UpdateData(data);
            if (Config.Mess)
            {
                plr.SendMessage($"\n玩家 [c/4EA4F2:{plr.Name}] 疾速[c/FF5265:冷却]完毕!", 244, 255, 150);

                if (Config.Dash)
                {
                    plr.SendMessage("双击[c/47DCBD:冲刺]可冲更远", 244, 255, 150);
                }

                if (Config.Jump)
                {
                    plr.SendMessage("装备指定物品[c/47DCBC:可加速跳跃]!", 244, 255, 150);
                }
            }
        }

        //使用了多少次进入冷却
        if(data.Enabled && data.Count >= Config.Count)
        {
            data.Enabled = false;
            data.CoolTime = now;
            DB.UpdateData(data);
            if (Config.Mess)
            {
                plr.SendMessage($"\n玩家 [c/4EA4F2:{plr.Name}] 因[c/FF5265:冲刺]或[c/DBF34E:跳跃] 超过[c/47DCBD:{Config.Count}次]进入冷却", 244, 255, 150);
            }
            return;
        }

        //冲刺
        if (Config.Dash && data.Enabled && CheckDash(tplr, data, CLeft, CRight))
        {
            if (CRight)
            {
                tplr.velocity.X = Config.Speed * Config.Multiple;
                data.Count++;
            }

            else if (CLeft)
            {
                tplr.velocity.X = -Config.Speed * Config.Multiple;
                data.Count++;
            }

            DB.UpdateData(data);
            TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", plr.Index, 0f, 0f, 0f, 0);
        }

        //跳跃
        if (Config.Jump && data.Enabled && armor && CheckJupm(tplr, data, CLeft, CRight))
        {
            if (CRight)
            {
                tplr.velocity.X = Config.Speed;
                data.Count++;
            }

            else if (CLeft)
            {
                tplr.velocity.X = -Config.Speed;
                data.Count++;
            }

            DB.UpdateData(data);
            TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", plr.Index, 0f, 0f, 0f, 0);
        }
    }
    #endregion

    #region 检查玩家跳跃间隔方法
    private static bool CheckJupm(Player tplr, Database.PlayerData data, bool CLeft, bool CRight)
    {
        var now = DateTime.UtcNow;
        var Jump = tplr.controlJump;
        if ((now - data.RangeTime).TotalMilliseconds < Config.Range) return false;

        if ((Jump && CRight) || (CLeft && Jump))
        {
            data.RangeTime = now; // 更新最后一次跳跃的时间
            return true;
        }

        return false;
    }
    #endregion

    #region 检查玩家冲刺间隔方法
    private static bool CheckDash(Player tplr, Database.PlayerData data, bool CLeft, bool CRight)
    {
        var now = DateTime.UtcNow;
        var dash = tplr.dashDelay == -1;
        if ((now - data.RangeTime).TotalMilliseconds < Config.Range) return false;

        if ((dash && CRight) || (CLeft && dash))
        {
            data.RangeTime = now; // 更新最后一次跳跃的时间
            return true;
        }

        return false;
    }
    #endregion
}