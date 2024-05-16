namespace PrintToPDFNode

{
    public interface IPdfConversionStrategy
    {
        public ToPdfResp ConvertToPdf(string filePath);
    }

}
