using System;
using System.Collections.Generic;

namespace TDEV_811.Models;

public partial class MqttMessage
{
    public int Id { get; set; }

    public string? Topic { get; set; }

    public string? Payload { get; set; }
}
