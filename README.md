# PlayerSpeed 玩家速度插件

- 作者: 逆光奔跑 羽学
- 出处: Tshock官方Q群816771079
- 这是一个Tshock服务器插件，主要用于：冲刺时提高玩家速度与距离

## 更新日志

```
v1.1.0
因群友"哨兵"服主的定制要求修改而来
加入了冲刺判断逻辑和冷却机制
重构了大部分代码与指令方法、触发逻辑
玩家使用权限：vel.use
管理员权限：vel.admin
/vel set 指令格式：
/vel s sd 40 t 10

v1.0.0
从逆光奔跑那反编译来的
```

## 指令

| 语法                             | 别名  |       权限       |                   说明                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /vel on  | 无 |   vel.admin    |    开启插件功能    |
| /vel off | 无 |   vel.admin    |    关闭插件功能    |
| /vel set | /vel s |   vel.admin    |    设置全局冲刺速度与冷却时间    |
| /vel mess | 无 |   vel.admin    |    开启或关闭播报系统    |
| /vel del | 无 |   vel.admin    |    删除指定玩家数据    |
| /vel reset | 无 |   vel.admin    |    重置所有玩家数据    |
| /reload  | 无 |   tshock.cfg.reload    |    重载配置文件    |

## 配置
> 配置文件位置：tshock/玩家速度.json
```json
{
  "插件开关": true,
  "冷却时间": 20,
  "速度": 40.0,
  "播报": true
}
```
## 反馈
- 优先发issued -> 共同维护的插件库：https://github.com/UnrealMultiple/TShockPlayerSpeed
- 次优先：TShock官方群：816771079
- 大概率看不到但是也可以：国内社区trhub.cn ，bbstr.net , tr.monika.love