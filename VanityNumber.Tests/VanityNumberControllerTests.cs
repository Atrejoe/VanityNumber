using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VanityNumber.Api.Controllers;
using VanityNumber.Core.Models;
using VanityNumber.Core.Services;

namespace VanityNumber.Tests;


#pragma warning disable CS1591 // Missing XML comment : method should be self explanatory (for now)
public class VanityNumberControllerTests
{
    private readonly Mock<IVanityNumberService> _mockVanityService;
    private readonly Mock<IDictionaryService> _mockDictionaryService;
    private readonly Mock<IPhoneToLetterMapper> _mockLetterMapper;
    private readonly Mock<ILogger<VanityNumberController>> _mockLogger;
    private readonly VanityNumberController _controller;

    public VanityNumberControllerTests()
    {
        _mockVanityService = new Mock<IVanityNumberService>();
        _mockDictionaryService = new Mock<IDictionaryService>();
        _mockLetterMapper = new Mock<IPhoneToLetterMapper>();
        _mockLogger = new Mock<ILogger<VanityNumberController>>();
        _controller = new VanityNumberController(
            _mockVanityService.Object,
            _mockDictionaryService.Object,
            _mockLetterMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void GetDictionaries_ShouldReturnListOfDictionaries()
    {
        // Arrange
        _mockDictionaryService.Setup(x => x.GetWordCount(DictionaryType.Dutch)).Returns(12345);
        _mockDictionaryService.Setup(x => x.GetWordCount(DictionaryType.English)).Returns(54321);
        _mockDictionaryService.Setup(x => x.GetWordCount(DictionaryType.Urban)).Returns(9876);

        // Act
        var result = _controller.GetDictionaries();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dictionaries = Assert.IsAssignableFrom<IEnumerable<DictionaryInfo>>(okResult.Value);
        var dictionaryList = dictionaries.ToList();
        
        Assert.Equal(3, dictionaryList.Count);
        
        var dutch = dictionaryList.First(d => d.Name == "Dutch");
        Assert.Equal(1, dutch.Value);
        Assert.Equal("Dutch language dictionary", dutch.Description);
        Assert.Equal(12345, dutch.WordCount);
        
        var english = dictionaryList.First(d => d.Name == "English");
        Assert.Equal(2, english.Value);
        Assert.Equal("English language dictionary", english.Description);
        Assert.Equal(54321, english.WordCount);
        
        var urban = dictionaryList.First(d => d.Name == "Urban");
        Assert.Equal(4, urban.Value);
        Assert.Equal("Urban slang dictionary", urban.Description);
        Assert.Equal(9876, urban.WordCount);
    }

    [Fact]
    public void GetDictionaries_ShouldReturnCorrectDictionaryTypeValues()
    {
        // Arrange
        _mockDictionaryService.Setup(x => x.GetWordCount(It.IsAny<DictionaryType>())).Returns(100);

        // Act
        var result = _controller.GetDictionaries();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dictionaries = Assert.IsAssignableFrom<IEnumerable<DictionaryInfo>>(okResult.Value);
        var dictionaryList = dictionaries.ToList();
        
        // Verify the enum values match the DictionaryType flags
        Assert.Contains(dictionaryList, d => d.Value == (int)DictionaryType.Dutch);
        Assert.Contains(dictionaryList, d => d.Value == (int)DictionaryType.English);
        Assert.Contains(dictionaryList, d => d.Value == (int)DictionaryType.Urban);
    }

    [Fact]
    public void GetDictionaries_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _mockDictionaryService.Setup(x => x.GetWordCount(It.IsAny<DictionaryType>()))
            .Throws(new Exception("Test exception"));

        // Act
        var result = _controller.GetDictionaries();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }
    
    [Fact]
    public void ConvertVanityToDigits_ShouldReturnDigits()
    {
        // Arrange
        var vanityNumber = "8atm4n";
        var expectedDigits = "828646";
        _mockLetterMapper.Setup(x => x.ConvertVanityToDigits(vanityNumber)).Returns(expectedDigits);

        // Act
        var result = _controller.ConvertVanityToDigits(vanityNumber);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var digits = Assert.IsType<string>(okResult.Value);
        Assert.Equal(expectedDigits, digits);
    }
    
    [Fact]
    public void ConvertVanityToDigits_WithEmptyString_ShouldReturnBadRequest()
    {
        // Act
        var result = _controller.ConvertVanityToDigits("");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
