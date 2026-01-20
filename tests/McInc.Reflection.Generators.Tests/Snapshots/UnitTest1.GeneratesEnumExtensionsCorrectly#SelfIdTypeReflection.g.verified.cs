//HintName: SelfIdTypeReflection.g.cs
namespace Test12345;

internal sealed class SelfIdTypeReflection
{
	public SelfIdTypeReflection(Test12345.SelfId @object)
	{
		this._object = @object;
	}

	public string[] Members { get; } = new string[] {
		nameof(Test12345.SelfId.Id),
		nameof(Test12345.SelfId.Id3),
		nameof(Test12345.SelfId.A),
		nameof(Test12345.SelfId.B),
		nameof(Test12345.SelfId.C),
		nameof(Test12345.SelfId.D)
	};

	public bool TryGetValue(string name, out object? value)
	{
		switch(name)
		{
			case nameof(Test12345.SelfId.Id):
				value = this._object.Id;
				return true;

			case nameof(Test12345.SelfId.Id3):
				value = this._object.Id3;
				return true;

			case nameof(Test12345.SelfId.A):
				value = this._object.A;
				return true;

			case nameof(Test12345.SelfId.B):
				value = this._object.B;
				return true;

			case nameof(Test12345.SelfId.D):
				value = this._object.D;
				return true;

			default:
				value = null;
				return false;
		}
	}

	public bool TryGetValue<TValue>(string name, out TValue? value)
	{
		if (!this.TryGetValue(name, out var objValue)
			|| objValue is not T v)
		{
			value = default;
			return false;
		}

		value = v;
		return true;
	}

		private readonly Test12345.SelfId _object;

}
