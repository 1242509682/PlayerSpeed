using System.Text;
using Microsoft.Xna.Framework;
using TShockAPI;
using static PlayerSpeed.PlayerSpeed;

namespace PlayerSpeed;

public class Commands
{
    #region 主体指令方法
    public static void vel(CommandArgs args)
    {
        var plr = args.Player;

        if (!Config.Enabled || plr == null)
        {
            return;
        }

        if (args.Parameters.Count == 0)
        {
            HelpCmd(plr);
            return;
        }

        if (args.Parameters.Count >= 1)
        {
            switch (args.Parameters[0].ToLower())
            {
                case "on":
                    Config.Enabled = true;
                    Config.Write();
                    plr.SendInfoMessage($"玩家 [{plr.Name}] 已[c/92C5EC:启用]加速功能。");
                    break;

                case "off":
                    Config.Enabled = false;
                    Config.Write();
                    plr.SendInfoMessage($"玩家 [{plr.Name}] 已[c/92C5EC:关闭]加速功能。");
                    break;

                case "mess":
                    Config.SendMess = !Config.SendMess;
                    plr.SendSuccessMessage(Config.SendMess ?
                        $"玩家 [{plr.Name}] 的已[c/92C5EC:启用]冲刺播报" :
                        $"玩家 [{plr.Name}] 的已[c/92C5EC:关闭]冲刺播报" );
                    Config.Write();
                    break;

                case "del":
                    if (args.Parameters.Count >= 1)
                    {
                        var other = args.Parameters[1]; // 指定的玩家名字
                        DB.DeleteData(other);
                        plr.SendSuccessMessage($"已[c/E8585B:删除] {other} 的数据！");
                    }
                    return;

                case "reset":
                        DB.ClearData();
                        plr.SendInfoMessage("已清除所有玩家数据。");
                    break;

                case "set":
                case "s":
                    if (args.Parameters.Count >= 2)
                    {
                        Dictionary<string, string> ItemVal = new Dictionary<string, string>();
                        Parse(args.Parameters, out ItemVal, 1);
                        UpdatePT(ItemVal);
                    }
                    else
                    {
                        plr.SendInfoMessage("格式为:/vel s sd 20 sj 120\n" +
                            "确保参数后有一个正确的数字");
                    }
                    break;
                default:
                    HelpCmd(plr);
                    break;
            }
            return;
        }
    }


    #endregion

    #region 解析输入参数的属性名 通用方法
    private static void UpdatePT(Dictionary<string, string> itemValues)
    {
        var mess = new StringBuilder();
        mess.Append($"修改冲刺:");
        foreach (var kvp in itemValues)
        {
            string propName;
            switch (kvp.Key.ToLower())
            {
                case "sd":
                case "speed":
                case "速度":
                    if (float.TryParse(kvp.Value, out float speed)) Config.Speed = speed;
                    propName = "速度";
                    break;
                case "t":
                case "sj":
                case "time":
                case "时间":
                    if (int.TryParse(kvp.Value, out int t)) Config.CoolTime = t;
                    propName = "冷却时间";
                    break;
                default:
                    propName = kvp.Key;
                    break;
            }
            mess.AppendFormat("[c/94D3E4:{0}]:[c/FF6975:{1}] ", propName, kvp.Value);
        }

        Config.Write();
        TShock.Utils.Broadcast(mess.ToString(), 255, 244, 150);
    }
    #endregion

    #region 解析输入参数的距离 如:da 1
    private static void Parse(List<string> parameters, out Dictionary<string, string> itemValues, int Index)
    {
        itemValues = new Dictionary<string, string>();
        for (int i = Index; i < parameters.Count; i += 2)
        {
            if (i + 1 < parameters.Count) // 确保有下一个参数
            {
                string propertyName = parameters[i].ToLower();
                string value = parameters[i + 1];
                itemValues[propertyName] = value;
            }
        }
    }
    #endregion

    #region 菜单方法
    private static void HelpCmd(TSPlayer plr)
    {
        if (plr == null)
        {
            return;
        }

        plr.SendMessage("[i:3455][c/AD89D5:玩][c/D68ACA:家][c/DF909A:速][c/E5A894:度][i:3454]\n" +
                        "/vel on ——开启插件功能\n" +
                        "/vel off ——关闭插件功能\n" +
                        "/vel s ——设置全局冲刺速度\n" +
                        "/vel mess ——播报系统开关\n" +
                        "/vel del 玩家名 ——删除玩家数据\n" +
                        "/vel reset ——清除所有数据", Color.AntiqueWhite);

        if (plr != null)
        {
            plr.SendInfoMessage($"冲刺速度:{Config.Speed} 冷却时间:{Config.CoolTime}");
        }
    }
    #endregion
}