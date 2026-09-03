using FormBuilder.Domain.Common;

namespace FormBuilder.Domain.Tests.Common;

public class EntityTests
{
    private sealed class SampleEntity : Entity
    {
        public SampleEntity(Guid id)
        {
            Id = id;
        }
    }

    private sealed class OtherEntity : Entity
    {
        public OtherEntity(Guid id)
        {
            Id = id;
        }
    }

    [Fact]
    public void Same_type_and_id_are_equal()
    {
        var id = Guid.NewGuid();
        var left = new SampleEntity(id);
        var right = new SampleEntity(id);

        left.Equals(right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Different_ids_are_not_equal()
    {
        var left = new SampleEntity(Guid.NewGuid());
        var right = new SampleEntity(Guid.NewGuid());

        left.Equals(right).Should().BeFalse();
        (left == right).Should().BeFalse();
    }

    [Fact]
    public void Different_types_with_the_same_id_are_not_equal()
    {
        var id = Guid.NewGuid();

        new SampleEntity(id).Equals(new OtherEntity(id)).Should().BeFalse();
    }

    [Fact]
    public void An_entity_is_never_equal_to_null()
    {
        new SampleEntity(Guid.NewGuid()).Equals(null).Should().BeFalse();
    }
}
