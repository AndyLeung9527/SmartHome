namespace Mail.Api.Options;

public class EmailOption
{
    public List<EmailOption_Item> Items { get; set; } = new();
}


public class EmailOption_Item
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public List<EmailOption_Item_Address> Addresses { get; set; } = new();
}

public class EmailOption_Item_Address
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Enable { get; set; }
}
