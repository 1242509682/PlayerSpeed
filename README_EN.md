# PlayerSpeed

- Authors: 逆光奔跑 羽学
- Source: TShock QQ Group 816771079
- This is a Tshock server PlayerSpeed, mainly used for：
- Increase the speed and distance of player dashes and jumps.
- Automatically place players in a cooldown period after they have performed the specified number of actions as defined in the configuration file.

## Update Log

```
v1.2.1
Refactored the infinite dash logic to avoid performance issues:
- Enters cooldown after reaching the configured 'count' of actions.
- Added interval time (in milliseconds) between each action.
- Removed "last jump" related announcements.
- Removed stop time (ut) attribute.
- Added new property parameters to the `/vel s` command: `interval:r`, `count:c`.

v1.2.0
Infinite Sprint Mechanism Added:
When using shield-type accessories, double-tap to sprint continuously.
Equipping specified items allows continuous jumping, which can refresh the infinite sprint interval.
If the sprint interval exceeds the milliseconds defined as Stop Infinite Sprint, it will automatically enter a cooldown period.

New Attributes Added to /vel s Command:
Stop Time (ut)
Add Jump Item (add)
Remove Jump Item (del)

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
  "Enabled": true,
  "Count": 5,
  "Interval": 2000.0,
  "Cooldown": 25,
  "Speed": 30.0,
  "SendMessages": true,
  "DashEnabled": true,
  "DashSpeedMultiplier": 2.0,
  "JumpSpeedUpdate": true,
  "JumpSpeedItems": [
    5107,
    4989
  ]
}
```
## FeedBack
- Github Issue -> TShockPlayerSpeed Repo: https://github.com/UnrealMultiple/TShockPlayerSpeed
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love