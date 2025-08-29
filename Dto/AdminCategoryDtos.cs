public class BulkCreateCategoryRequest
{
	public List<CreateCategoryDto> Categories { get; set; } = new();
}

public class CreateCategoryDto
{
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
}
