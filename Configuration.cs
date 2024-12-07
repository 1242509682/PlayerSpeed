using Newtonsoft.Json;
using TShockAPI;

namespace PlayerSpeed
{
    internal class Configuration
    {
        #region 实例变量
        [JsonProperty("开关", Order = 0)]
        public bool Enabled { get; set; } = true;
        [JsonProperty("次数", Order = 1)]
        public int Count { get; set; } = 5;
        [JsonProperty("间隔", Order = 2)]
        public double Range { get; set; } = 2000;
        [JsonProperty("冷却", Order = 3)]
        public int CoolTime { get; set; } = 25;
        [JsonProperty("速度", Order = 4)]
        public float Speed { get; set; } = 20.0f;
        [JsonProperty("高度", Order = 4)]
        public float Height { get; set; } = 5.0f;
        [JsonProperty("播报", Order = 5)]
        public bool Mess { get; set; } = true;

        [JsonProperty("冲刺", Order = 10)]
        public bool Dash { get; set; } = true;
        [JsonProperty("冲刺速度倍数", Order = 11)]
        public float Multiple { get; set; } = 1.5f;

        [JsonProperty("跳跃", Order = 20)]
        public bool Jump { get; set; } = true;
        [JsonProperty("跳跃下降除于倍数", Order = 21)]
        public float Multiple2 { get; set; } = 5.0f;
        [JsonProperty("跳跃加速物品", Order = 22)]
        public List<int>? ArmorItem { get; set; }

        [JsonProperty("自动进度", Order = 31)]
        public bool KilledBoss { get; set; } = true;
        [JsonProperty("自动进度表", Order = 32)]
        public List<BossData> BossList { get; set; } = new List<BossData>();
        #endregion

        #region 进度表结构
        public class BossData
        {
            [JsonProperty("怪物名称", Order = 0)]
            public string Name { get; set; } = "";
            [JsonProperty("击败状态", Order = 1)]
            public bool Enabled { get; set; }
            [JsonProperty("设置速度", Order = 2)]
            public float Speed { get; set; }
            [JsonProperty("设置高度", Order = 3)]
            public float Height { get; set; }
            [JsonProperty("使用次数", Order = 4)]
            public int Count { get; set; } = 5;
            [JsonProperty("冷却时间", Order = 5)]
            public int CoolTime { get; set; } = 25;
            [JsonProperty("怪物ID", Order = 6)]
            public int[] ID { get; set; }

            public BossData(string name,bool enabled, int count, int coolTime, float speed,float height,int[] id)
            {
                Name = name ?? "";
                this.Enabled = enabled;
                this.Speed = speed;
                this.Height = height;
                this.Count = count;
                this.CoolTime = coolTime;
                this.ID = id ?? new int[] { 1 };
            }
        }
        #endregion

        #region 预设参数方法
        private void New()
        {
            this.ArmorItem = new List<int>() { 5107, 4989 };

            this.BossList = new List<BossData>
            {
                new BossData("",false,1,60,20f,2.5f,new int []{ 4,50 }),
                new BossData("",false,2,45,25f,5f,new int []{ 13,266 }),
                new BossData("",false,3,30,30f,10f,new int []{ 113 }),
                new BossData("",false,4,15,40f,15f, new int[] { 125, 126, 127, 134 })
            };
        } 
        #endregion

        #region 读取与创建配置文件方法
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "玩家速度.json");
        public void Write()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented); 
            File.WriteAllText(FilePath, json);
        }

        public static Configuration Read()
        {
            if (!File.Exists(FilePath))
            {
                var NewConfig = new Configuration();
                NewConfig.New();
                new Configuration().Write();
                return NewConfig;
            }
            else
            {
                var jsonContent = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
            }
        }
        #endregion
    }
}