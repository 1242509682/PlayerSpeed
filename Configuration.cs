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
        public float Speed { get; set; } = 30.0f;
        [JsonProperty("播报", Order = 5)]
        public bool Mess { get; set; } = true;

        [JsonProperty("冲刺", Order = 10)]
        public bool Dash { get; set; } = true;
        [JsonProperty("冲刺速度倍数", Order = 11)]
        public float Multiple { get; set; } = 2.0f;

        [JsonProperty("跳跃", Order = 20)]
        public bool Jump { get; set; } = true;
        [JsonProperty("跳跃加速物品", Order = 21)]
        public List<int>? ArmorItem { get; set; }

        #endregion

        #region 预设参数方法
        private void New()
        {
            ArmorItem = new List<int>() { 5107, 4989 };
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