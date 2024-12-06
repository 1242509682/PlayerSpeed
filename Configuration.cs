using Newtonsoft.Json;
using TShockAPI;

namespace PlayerSpeed
{
    internal class Configuration
    {
        #region 实例变量
        [JsonProperty("插件开关", Order = 0)]
        public bool Enabled { get; set; } = true;
        [JsonProperty("冷却秒数", Order = 1)]
        public int CoolTime { get; set; } = 20;
        [JsonProperty("冲刺速度", Order = 2)]
        public float Speed { get; set; } = 40;
        [JsonProperty("使用播报", Order = 3)]
        public bool SendMess { get; set; } = true;
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