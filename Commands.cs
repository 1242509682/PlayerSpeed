using System.Text;
using Microsoft.Xna.Framework;
using TShockAPI;
using static MonoMod.InlineRT.MonoModRule;
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

        if (args.Parameters.Count >= 1 && plr.HasPermission("vel.admin"))
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
                    Config.Mess = !Config.Mess;
                    plr.SendSuccessMessage(Config.Mess ?
                        $"玩家 [{plr.Name}] 的已[c/92C5EC:启用]玩家速度播报" :
                        $"玩家 [{plr.Name}] 的已[c/92C5EC:关闭]玩家速度播报");
                    Config.Write();
                    break;

                case "boss":
                    Config.KilledBoss = !Config.KilledBoss;
                    plr.SendSuccessMessage(Config.KilledBoss ?
                        $"玩家 [{plr.Name}] 的已[c/92C5EC:启用]自动进度模式" :
                        $"玩家 [{plr.Name}] 的已[c/92C5EC:关闭]自动进度模式");
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

                    if (Config.BossList != null && Config.KilledBoss)
                    {
                        for (int i = 0; i < Config.BossList.Count; i++)
                        {
                            if (Config.BossList[i].Enabled)
                            {
                                Config.BossList[i].Enabled = false;
                            }
                        }

                        Config.Write();
                        plr.SendInfoMessage("已清除所有玩家数据,重置进度模式。");
                    }
                    else
                    {
                        plr.SendInfoMessage("已清除所有玩家数据");
                    }

                    DB.ClearData();
                    break;

                case "set":
                case "s":
                    if (args.Parameters.Count >= 2)
                    {
                        Dictionary<string, string> ItemVal;
                        Parse(args.Parameters, out ItemVal, 1);
                        UpdatePT(args, ItemVal);
                    }
                    else
                    {
                        plr.SendMessage("参数: 速度([c/F24F62:sd]) 高度([c/48DCB8:h]) 间隔([c/4898DC:r]) 冷却([c/FE7F53:t])\n" +
                            "次数([c/DBF34E:c]) 加跳跃物品([c/59E32B:add]) 删跳跃物品([c/F14F63:del])\n" +
                            "格式为:[c/48DCB8:/vel s sd 20 add 恐慌项链…]\n" +
                            "确保属性后有正确的数字或名字,任意组合", 244, 255, 150);
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
    private static void UpdatePT(CommandArgs args, Dictionary<string, string> itemValues)
    {
        List<int> UpdateItem = new List<int>(Config.ArmorItem!);

        var mess = new StringBuilder();
        mess.Append($"修改冲刺:");
        foreach (var kvp in itemValues)
        {
            string prop;
            switch (kvp.Key.ToLower())
            {
                case "sd":
                case "speed":
                case "速度":
                    if (float.TryParse(kvp.Value, out float speed)) Config.Speed = speed;
                    prop = "速度";
                    break;
                case "gd":
                case "h":
                case "height":
                case "高度":
                    if (float.TryParse(kvp.Value, out float height)) Config.Height = height;
                    prop = "高度";
                    break;
                case "t":
                case "sj":
                case "time":
                case "冷却":
                case "时间":
                    if (int.TryParse(kvp.Value, out int t)) Config.CoolTime = t;
                    prop = "冷却";
                    break;
                case "c":
                case "ct":
                case "count":
                case "次数":
                    if (int.TryParse(kvp.Value, out int ct)) Config.Count = ct;
                    prop = "次数";
                    break;
                case "r":
                case "range":
                case "间隔":
                    if (double.TryParse(kvp.Value, out double r)) Config.Range = r;
                    prop = "间隔";
                    break;
                case "add":
                case "添加物品":
                    var add = TShock.Utils.GetItemByIdOrName(kvp.Value);
                    if (add.Count == 0)
                    {
                        args.Player.SendInfoMessage($"找不到名为 {kvp.Value} 的物品.");
                        return;
                    }
                    else if (add.Count > 1)
                    {
                        args.Player.SendMultipleMatchError(add.Select(i => i.Name));
                        return;
                    }

                    var newItemId = add[0].netID;
                    if (!UpdateItem.Contains(newItemId))
                    {
                        UpdateItem.Add(newItemId);
                        prop = $"添加物品";
                    }
                    else
                    {
                        prop = $"物品已存在";
                    }
                    break;
                case "del":
                case "移除物品":
                    if (int.TryParse(kvp.Value, out int remove))
                    {
                        if (UpdateItem.Remove(remove))
                        {
                            prop = $"移除物品";
                        }
                        else
                        {
                            prop = $"物品 {remove} 不存在.";
                        }
                    }
                    else
                    {
                        var ToRemove = TShock.Utils.GetItemByIdOrName(kvp.Value);
                        if (ToRemove.Count == 0)
                        {
                            args.Player.SendInfoMessage($"找不到名为 {kvp.Value} 的物品.");
                            return;
                        }
                        else if (ToRemove.Count > 1)
                        {
                            args.Player.SendMultipleMatchError(ToRemove.Select(i => i.Name));
                            return;
                        }

                        var del = ToRemove[0];
                        if (UpdateItem.Remove(del.netID))
                        {
                            prop = $"移除物品";
                        }
                        else
                        {
                            prop = $"物品不存在";
                        }
                    }
                    break;
                default:
                    prop = kvp.Key;
                    break;
            }
            mess.AppendFormat("[c/94D3E4:{0}]([c/FF6975:{1}]) ", prop, kvp.Value);
        }

        // 将修改后的列表复制回 触发跳跃加速的物品ID表
        Config.ArmorItem = new List<int>(UpdateItem);
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

        if (plr.HasPermission("vel.admin"))
        {
            plr.SendMessage("[i:3455][c/AD89D5:玩][c/D68ACA:家][c/DF909A:速][c/E5A894:度][i:3454]\n" +
                            "/vel on ——开启插件功能\n" +
                            "/vel off ——关闭插件功能\n" +
                            "/vel set ——设置相关参数\n" +
                            "/vel boss ——进度模式开关\n" +
                            "/vel mess ——播报系统开关\n" +
                            "/vel del 玩家名 ——删除玩家数据\n" +
                            "/vel reset ——清除所有数据", Color.AntiqueWhite);
        }

        if (!Config.KilledBoss)
        {
            plr.SendInfoMessage($"速度:[c/4EA4F2:{Config.Speed}] 高度:[c/FF5265:{Config.Height}] " +
                $"冷却:[c/48DCB8:{Config.CoolTime}] 间隔:[c/47DCBD:{Config.Range}]");
        }
        else
        {
            var boss = GetMaxSpeed(Config.BossList);
            if (boss != null && boss.Enabled)
            {
                plr.SendInfoMessage($"最高速度:[c/4EA4F2:{boss.Speed}] 最大高度:[c/FF5265:{boss.Height}] " +
                    $"冷却:[c/48DCB8:{boss.CoolTime}秒] 使用次数:[c/47DCBD:{boss.Count}秒]");
            }
            else
            {
                plr.SendInfoMessage("当前为进度模式,服务器未击败相关BOSS");
            }
        }
    }
    #endregion
}