using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace PlayerSpeed;

public class Database
{
    #region 数据的结构体
    public class PlayerData
    {
        //玩家名字
        public string Name { get; set; }
        //启动冲刺开关
        public bool Enabled { get; set; } = false;
        //正在冲刺开关
        public bool InUse { get; set; } = false;
        //冷却时间
        public DateTime CoolTime { get; set; }
        //使用时间
        public DateTime UseTime { get; set; }
        internal PlayerData(string name = "", bool enabled = true, bool inUse = true,DateTime coolTime = default, DateTime useTime = default)
        {
            this.Name = name ?? "";
            this.Enabled = enabled;
            this.InUse = inUse;
            this.CoolTime = coolTime == default ? DateTime.UtcNow : coolTime;
            this.UseTime = useTime == default ? DateTime.UtcNow : useTime;
        }
    }
    #endregion

    #region 数据库表结构（使用Tshock自带的数据库作为存储）
    public Database()
    {
        var sql = new SqlTableCreator(TShock.DB, new SqliteQueryCreator());

        // 定义并确保 PlayerSpeed 表的结构
        sql.EnsureTableStructure(new SqlTable("PlayerSpeed", //表名
            new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, AutoIncrement = true }, // 主键列
            new SqlColumn("Name", MySqlDbType.TinyText) { NotNull = true }, // 非空字符串列
            new SqlColumn("Enabled", MySqlDbType.Int32) { DefaultValue = "0" }, // bool值列
            new SqlColumn("InUse", MySqlDbType.Int32) { DefaultValue = "0" }, // bool值列
            new SqlColumn("CoolTime", MySqlDbType.DateTime), // 时间戳列，保留原字段名
            new SqlColumn("UseTime", MySqlDbType.DateTime) // 时间戳列，保留原字段名
        ));
    }
    #endregion

    #region 为玩家创建数据方法
    public bool AddData(PlayerData data)
    {
        return TShock.DB.Query("INSERT INTO PlayerSpeed (Name, Enabled, InUse, CoolTime, UseTime) VALUES (@0, @1, @2, @3, @4)",
            data.Name, data.Enabled ? 1 : 0, data.InUse ? 1 : 0,data.CoolTime, data.UseTime) != 0;
    }
    #endregion

    #region 删除指定玩家数据方法
    public bool DeleteData(string name)
    {
        return TShock.DB.Query("DELETE FROM PlayerSpeed WHERE Name = @0", name) != 0;
    }
    #endregion

    #region 更新数据内容方法
    public bool UpdateData(PlayerData data)
    {
        return TShock.DB.Query("UPDATE PlayerSpeed SET Enabled = @0, InUse = @1, CoolTime = @2, UseTime = @3 WHERE Name = @4",
            data.Enabled ? 1 : 0, data.InUse ? 1 : 0, data.CoolTime, data.UseTime, data.Name) != 0;
    }
    #endregion

    #region 获取数据方法
    public PlayerData? GetData(string name)
    {
        using var reader = TShock.DB.QueryReader("SELECT * FROM PlayerSpeed WHERE Name = @0", name);

        if (reader.Read())
        {
            return new PlayerData(
                name: reader.Get<string>("Name"),
                enabled: reader.Get<int>("Enabled") == 1,
                inUse: reader.Get<int>("InUse") == 1,
                coolTime: reader.Get<DateTime>("CoolTime"),
                useTime: reader.Get<DateTime>("UseTime")
            );
        }

        return null;
    }
    #endregion

    #region 获取所有玩家数据方法
    public List<PlayerData> GetAll()
    {
        var data = new List<PlayerData>();
        using var reader = TShock.DB.QueryReader("SELECT * FROM PlayerSpeed");
        while (reader.Read())
        {
            data.Add(new PlayerData(
                name: reader.Get<string>("Name"),
                enabled: reader.Get<int>("Enabled") == 1,
                inUse: reader.Get<int>("InUse") == 1,
                coolTime: reader.Get<DateTime>("CoolTime"),
                useTime: reader.Get<DateTime>("UseTime")
            ));
        }

        return data;
    }
    #endregion

    #region 清理所有数据方法
    public bool ClearData()
    {
        return TShock.DB.Query("DELETE FROM PlayerSpeed") != 0;
    }
    #endregion
}