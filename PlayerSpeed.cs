using Microsoft.Xna.Framework;
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
    public override Version Version => new Version(1, 2, 0);
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
                InUse = false,
                UseTime = DateTime.UtcNow,
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
        //冷却时间
        var LastCool = data.CoolTime != default ? (float)Math.Round((now - data.CoolTime).TotalSeconds, 2) : 0f;
        //使用时间
        var LastUse = data.UseTime != default ? (float)Math.Round((now - data.UseTime).TotalMilliseconds, 2) : 0f;

        //计算冷却过去多久
        if (!data.Enabled && LastCool >= Config.CoolTime)
        {
            data.Enabled = true;
            DB.UpdateData(data);
            if (Config.SendMess)
            {
                plr.SendMessage($"玩家 [c/4EA4F2:{plr.Name}] 疾速[c/FF5265:冲刺]冷却完毕!\n" +
                    $"在疾速时间不断[c/DBF34E:按方向]或[c/47DCBC:跳跃]可无限冲!", 244, 255, 150);
            }
        }

        // 检查是否因达到最大使用时间而结束冲刺
        if (data.InUse && LastUse >= Config.UseTime)
        {
            data.InUse = false;
            data.Enabled = false;
            data.CoolTime = now;
            DB.UpdateData(data);
            if (Config.SendMess)
            {
                plr.SendMessage($"玩家 [c/4EA4F2:{plr.Name}] 因停止疾速[c/FF5265:冲刺]或[c/DBF34E:跳跃]" +
                    $"\n超过[c/47DCBD:{Config.UseTime}毫秒]进入冷却", 244, 255, 150);
            }
            return;
        }

        var EverJump = CheckJupm(tplr);

        if (data.Enabled)
        {
            if ((Config.dashDelay && tplr.dashDelay == -1) || (Config.EverJump && EverJump))
            {
                if (tplr.direction == 1 && tplr.controlRight)
                {
                    tplr.velocity.X = Config.Speed;
                    tplr.velocity = new Vector2(Config.Speed, tplr.velocity.Y);
                    TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", plr.Index, 0f, 0f, 0f, 0);
                    if (Config.SendMess && Config.SendMess2)
                    {
                        plr.SendMessage($"玩家 [c/4EA4F2:{plr.Name}] [c/FF5265:上次]冲刺:[c/FFAE52:{LastUse}毫秒]", 244, 255, 150);
                    }

                    data.UseTime = now;
                    data.InUse = true;
                    DB.UpdateData(data);
                }

                else if (tplr.direction == -1 && tplr.controlLeft)
                {
                    tplr.velocity.X = -Config.Speed;
                    tplr.velocity = new Vector2(-Config.Speed, tplr.velocity.Y);
                    TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", plr.Index, 0f, 0f, 0f, 0);
                    if (Config.SendMess && Config.SendMess2)
                    {
                        plr.SendMessage($"玩家 [c/4EA4F2:{plr.Name}] [c/FF5265:上次]冲刺:[c/FFAE52:{LastUse}毫秒]", 244, 255, 150);
                    }

                    data.UseTime = now;
                    data.InUse = true;
                    DB.UpdateData(data);
                }
            }
        }
    }
    #endregion

    #region 检查玩家跳跃方法
    private static bool CheckJupm(Player tplr)
    {
        var click = 0;
        var EverJump = false;
        var CRight = tplr.controlRight && tplr.direction == 1;
        var CLeft = tplr.controlLeft && tplr.direction == -1;
        var armor = tplr.armor.Take(20).Any(x => Config.ArmorItem.Contains(x.netID));

        if (CRight || CLeft)
        {
            click++;
        }

        if (click >= 1 && tplr.controlJump && armor)
        {
            EverJump = true;
        }

        return EverJump;
    } 
    #endregion
}