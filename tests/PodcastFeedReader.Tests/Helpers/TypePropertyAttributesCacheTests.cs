namespace PodcastFeedReader.Tests.Helpers
{
    public class TypePropertyAttributesCacheTests
    {
        /*
        [Fact]
        public void GetExpectedProperties_ForClassWithNullString_DoesNotContainExpected()
        {
            var testObject = new GetExpectedPropertiesTestsImpl();
            testObject.StringValue = null;

            TypePropertyAttributesCache.AddTypesToCache(testObject.GetType());
            var expectedProperties = TypePropertyAttributesCache.GetExpectedProperties(testObject);

            Assert.DoesNotContain(nameof(IGetExpectedPropertiesTests.StringValue), expectedProperties);
        }

        [Fact]
        public void GetExpectedProperties_WithEmptyString_DoesNotReturnExpected()
        {
            var testObject = new GetExpectedPropertiesTestsImpl();
            testObject.StringValue = String.Empty;

            TypePropertyAttributesCache.AddTypesToCache(testObject.GetType());
            var expectedProperties = TypePropertyAttributesCache.GetExpectedProperties(testObject);

            Assert.DoesNotContain(nameof(IGetExpectedPropertiesTests.StringValue), expectedProperties);
        }

        [Fact]
        public void GetExpectedProperties_ForClassWithNullValue_DoesNotContainExpected()
        {
            var testObject = new GetExpectedPropertiesTestsImpl();
            testObject.IntNullableValue = null;

            TypePropertyAttributesCache.AddTypesToCache(testObject.GetType());
            var expectedProperties = TypePropertyAttributesCache.GetExpectedProperties(testObject);

            Assert.DoesNotContain(nameof(IGetExpectedPropertiesTests.IntNullableValue), expectedProperties);
        }

        [Fact]
        public void GetExpectedProperties_WithDefaultValue_DoesNotReturnExpected()
        {
            var testObject = new GetExpectedPropertiesTestsImpl();
            testObject.IntNullableValue = default(int);

            TypePropertyAttributesCache.AddTypesToCache(testObject.GetType());
            var expectedProperties = TypePropertyAttributesCache.GetExpectedProperties(testObject);

            Assert.DoesNotContain(nameof(IGetExpectedPropertiesTests.IntNullableValue), expectedProperties);
        }

        [Fact]
        public void GetExpectedProperties_ForClassWithNullReference_IsNotExpected()
        {
            var testObject = new GetExpectedPropertiesTestsImpl();
            testObject.ObjectValue = null;

            TypePropertyAttributesCache.AddTypesToCache(testObject.GetType());
            var expectedProperties = TypePropertyAttributesCache.GetExpectedProperties(testObject);

            Assert.DoesNotContain(nameof(IGetExpectedPropertiesTests.ObjectValue), expectedProperties);
        }

        [Fact]
        public void GetExpectedProperties_ForClassWithInitialisedReference_IsNotExpected()
        {
            var testObject = new GetExpectedPropertiesTestsImpl();
            testObject.ObjectValue = new object();

            TypePropertyAttributesCache.AddTypesToCache(testObject.GetType());
            var expectedProperties = TypePropertyAttributesCache.GetExpectedProperties(testObject);

            Assert.DoesNotContain(nameof(IGetExpectedPropertiesTests.ObjectValue), expectedProperties);
        }

        interface IGetExpectedPropertiesTests
        {
            [Expected]
            string StringValue { get; set; }

            [Expected]
            int? IntNullableValue { get; set; }

            [Expected]
            int IntValue { get; set; }

            [Expected]
            object ObjectValue { get; set; }
        }

        private class GetExpectedPropertiesTestsImpl : IGetExpectedPropertiesTests
        {
            public string StringValue { get; set; }
            public int? IntNullableValue { get; set; }
            public int IntValue { get; set; }
            public object ObjectValue { get; set; }
        }
        */
    }
}