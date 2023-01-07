namespace TabaloniaNew.Exceptions;

public class ElementNotFoundOnStyleException : Exception
{
    public ElementNotFoundOnStyleException(string elementName) : base($"\"{elementName}\" not found on Style")
    {
    }
}