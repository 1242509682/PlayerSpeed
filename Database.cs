using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace PlayerSpeed;

public class Database
{
    #region ���ݵĽṹ��
    public class PlayerData
    {
        //�������
        public string Name { get; set; }
        //������̿���
        public bool Enabled { get; set; } = false;
        //���ڳ�̿���
        public bool InUse { get; set; } = false;
        //��ȴʱ��
        public DateTime CoolTime { get; set; }
        //ʹ��ʱ��
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

    #region ���ݿ��ṹ��ʹ��Tshock�Դ������ݿ���Ϊ�洢��
    public Database()
    {
        var sql = new SqlTableCreator(TShock.DB, new SqliteQueryCreator());

        // ���岢ȷ�� PlayerSpeed ��Ľṹ
        sql.EnsureTableStructure(new SqlTable("PlayerSpeed", //����
            new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, AutoIncrement = true }, // ������
            new SqlColumn("Name", MySqlDbType.TinyText) { NotNull = true }, // �ǿ��ַ�����
            new SqlColumn("Enabled", MySqlDbType.Int32) { DefaultValue = "0" }, // boolֵ��
            new SqlColumn("InUse", MySqlDbType.Int32) { DefaultValue = "0" }, // boolֵ��
            new SqlColumn("CoolTime", MySqlDbType.DateTime), // ʱ����У�����ԭ�ֶ���
            new SqlColumn("UseTime", MySqlDbType.DateTime) // ʱ����У�����ԭ�ֶ���
        ));
    }
    #endregion

    #region Ϊ��Ҵ������ݷ���
    public bool AddData(PlayerData data)
    {
        return TShock.DB.Query("INSERT INTO PlayerSpeed (Name, Enabled, InUse, CoolTime, UseTime) VALUES (@0, @1, @2, @3, @4)",
            data.Name, data.Enabled ? 1 : 0, data.InUse ? 1 : 0,data.CoolTime, data.UseTime) != 0;
    }
    #endregion

    #region ɾ��ָ��������ݷ���
    public bool DeleteData(string name)
    {
        return TShock.DB.Query("DELETE FROM PlayerSpeed WHERE Name = @0", name) != 0;
    }
    #endregion

    #region �����������ݷ���
    public bool UpdateData(PlayerData data)
    {
        return TShock.DB.Query("UPDATE PlayerSpeed SET Enabled = @0, InUse = @1, CoolTime = @2, UseTime = @3 WHERE Name = @4",
            data.Enabled ? 1 : 0, data.InUse ? 1 : 0, data.CoolTime, data.UseTime, data.Name) != 0;
    }
    #endregion

    #region ��ȡ���ݷ���
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

    #region ��ȡ����������ݷ���
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

    #region �����������ݷ���
    public bool ClearData()
    {
        return TShock.DB.Query("DELETE FROM PlayerSpeed") != 0;
    }
    #endregion
}