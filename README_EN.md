# PlayerSpeed

- Authors: 逆光奔跑 羽学
- Source: TShock QQ Group 816771079
- This is a Tshock server PlayerSpeed, mainly used for：increase player speed and distance during dashes.

## Update Log

```
v1.1.0
Modified based on customization requirements by the server owner "Sentinel" from the group.
Added dash logic and cooldown mechanism.
Refactored most of the code, command methods, and trigger logic.
Player permission: vel.use
Admin permission: vel.admin
Command format for /vel set:
/vel s sd 40 t 10

v1.0.0
Decompiled from Niguang Benpao's version.
```

## Commands

| Syntax                             | Alias  |       Permission       |                   Description                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /vel on                          | None  | vel.admin    | Enable the plugin function           |
| /vel off                         | None  | vel.admin    | Disable the plugin function          |
| /vel set                         | /vel s| vel.admin    | Set global dash speed and cooldown time |
| /vel mess                        | None  | vel.admin    | Toggle announcement system            |
| /vel del                         | None  | vel.admin    | Delete specified player data          |
| /vel reset                       | None  | vel.admin    | Reset all player data                 |
| /reload                          | None  | tshock.cfg.reload | Reload configuration file |

## Configuration
> Configuration file location： tshock/玩家速度.json
```json
{
"PluginEnabled": true,
  "CooldownTime": 20,
  "Speed": 40.0,
  "Announcement": true
}
```
## FeedBack
- Github Issue -> TShockPlayerSpeed Repo: https://github.com/UnrealMultiple/TShockPlayerSpeed
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love