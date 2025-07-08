namespace TDEV_811.DTO
{
    public class MqttMessageDTO
    {
        public int Id { get; set; }

        public string? Topic { get; set; }

        public string? Payload { get; set; }
    }
}
