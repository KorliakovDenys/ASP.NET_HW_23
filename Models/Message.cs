using Newtonsoft.Json;

namespace ASP.NET_HW_23.Models;

public class Message {
    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;

    [JsonProperty("sender")]
    public string Sender { get; set; } = string.Empty;

    [JsonProperty("recipient")]
    public string Recipient { get; set; } = string.Empty;
}