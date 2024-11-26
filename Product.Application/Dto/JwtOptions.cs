namespace Product.Application.Dto;

public class JwtOptions
{
	public string Secret { get; set; } = string.Empty;
	public int ExpireHours { get; set; }
	public string Issuer { get; set; } = string.Empty;
	public string Audience { get; set; } = string.Empty;
}
