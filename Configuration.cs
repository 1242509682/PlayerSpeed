using Newtonsoft.Json;
using TShockAPI;

namespace PlayerSpeed
{
    internal class Configuration
    {
        #region 实例变量
        [JsonProperty("插件开关", Order = 0)]
        public bool Enabled { get; set; } = true;
        [JsonProperty("开启无限冲冷却秒数", Order = 1)]
        public int CoolTime { get; set; } = 35;
        [JsonProperty("停止无限冲时的毫秒", Order = 2)]
        public int UseTime { get; set; } = 1000;
        [JsonProperty("冲刺速度", Order = 2)]
        public float Speed { get; set; } = 20;
        [JsonProperty("全局播报", Order = 3)]
        public bool SendMess { get; set; } = true;
        [JsonProperty("克盾类双击冲刺加速", Order = 4)]
        public bool dashDelay { get; set; } = true;
        [JsonProperty("无限冲刺的间隔播报", Order = 5)]
        public bool SendMess2 { get; set; } = true;
        [JsonProperty("装备以下物品跳跃加速", Order = 6)]
        public bool EverJump { get; set; } = true;
        [JsonProperty("触发跳跃加速的物品ID", Order = 7)]
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