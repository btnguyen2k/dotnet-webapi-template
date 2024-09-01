namespace Dwt.Api.Bootstrap;


/// <summary>
/// Class-level attribute to mark a class as a bootstrapper.
/// </summary>
/// <remarks></remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BootstrapperAttribute : Attribute
{
	/// <summary>
	/// Bootstrapper priority, lower value means higher priority.
	/// Bootstrappers with higher priority will be executed first.
	/// </summary>
	public int Priority { get; set; } = 1000;
}
