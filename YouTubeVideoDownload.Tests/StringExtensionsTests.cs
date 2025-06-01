using Xunit;
using YouTubeVideoDownload.Extensions;

namespace YouTubeVideoDownload.Tests
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("Simple Title", "simple-title")]
        [InlineData("Title with spaces", "title-with-spaces")]
        [InlineData("Title with UPPERCASE", "title-with-uppercase")]
        [InlineData("Title with special chars !@#$%^&*()", "title-with-special-chars")]
        [InlineData("Título com acentuação é ç à", "titulo-com-acentuacao-e-c-a")]
        [InlineData("A very long title that will be truncated eventually at some point because it exceeds the forty-five character limit", "a-very-long-title-that-will-be-truncated-e")]
        [InlineData("  Title with leading/trailing spaces  ", "title-with-leadingtrailing-spaces")]
        [InlineData("重複のタイトル", "重复のタイトル")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void GenerateSlug_ReturnsCorrectSlug(string input, string expected)
        {
            // Act
            var result = (input != null ? StringExtensions.GenerateSlug(input) : string.Empty);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Olá Mundo", "Ola Mundo")]
        [InlineData("Título com acentuação é ç à", "Titulo com acentuacao e c a")]
        [InlineData("Crème brûlée", "Creme brulee")]
        [InlineData("你好世界", "你好世界")]
        [InlineData("Simple Text", "Simple Text")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void RemoveAccent_ReturnsStringWithoutAccents(string input, string expected)
        {
            // Act
            var result = (input != null ? StringExtensions.RemoveAccent(input) : string.Empty);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
