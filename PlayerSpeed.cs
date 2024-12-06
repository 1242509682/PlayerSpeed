using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace PlayerSpeed;

[ApiVersion(2, 1)]
public class PlayerSpeed : TerrariaPlugin
{
    #region 插件模版信息
    public override string Name => "玩家速度";
    public override string Author => "逆光奔跑 羽学";
    public override Version Version => new Version(1, 1, 0);
    public override string Description => "使用指令设置玩家移动速度 并在冲刺时触发";
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
                Enabled = true,
                Time = DateTime.UtcNow,
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
        var last = 0f;
        if (data.Time != default)
        {
            last = (float)Math.Round((now - data.Time).TotalSeconds, 2);
        }

        if (!data.Enabled && last >= Config.CoolTime)
        {
            data.Enabled = true;
            DB.UpdateData(data);
            if (Config.SendMess)
            {
                plr.SendMessage($"玩家 [c/4EA4F2:{plr.Name}] 疾速[c/FF5265:冲刺]冷却完毕!", 244, 255, 150);
            }
        }

        if (data.Enabled && tplr.dashDelay == -1)
        {
            if (tplr.direction == 1 && tplr.controlRight)
            {
                tplr.velocity.X = Config.Speed;
                tplr.velocity = new Vector2(Config.Speed, tplr.velocity.Y);
                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", plr.Index, 0f, 0f, 0f, 0);
                if (Config.SendMess)
                {
                    plr.SendMessage($"玩家 [c/4EA4F2:{plr.Name}] 疾速[c/FF5265:冲刺]结束! 上次冲刺过去了[c/FFAE52:{last}秒]", 244, 255, 150);
                }
                data.Enabled = false;
                data.Time = now;
                DB.UpdateData(data);
            }
            else if (tplr.direction == -1 && tplr.controlLeft)
            {
                tplr.velocity.X = -Config.Speed;
                tplr.velocity = new Vector2(-Config.Speed, tplr.velocity.Y);
                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", plr.Index, 0f, 0f, 0f, 0);
                if (Config.SendMess)
                {
                    plr.SendMessage($"玩家 [c/4EA4F2:{plr.Name}] 疾速[c/FF5265:冲刺]结束! 上次冲刺过去了[c/FFAE52:{last}秒]", 244, 255, 150);
                }
                data.Enabled = false;
                data.Time = now;
                DB.UpdateData(data);
            }
        }
    }
    #endregion
}