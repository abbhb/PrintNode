using NewLife.Serialization.Json;

namespace PrintNode
{
    public class Test
    {
        public static void test() {
            string mufile = "D:\\open\\publishTest\\UI学习方向-副本.pdf";
            PrintDataPDFToPrintReq json = new PrintDataPDFToPrintReq();
            json.copies = 1;
            json.id = "1";
            json.name = "UI学习方向-副本.pdf";
            json.isDuplex = 0;
            json.startNum = 1;
            json.endNum = 2;

            ToPrint.print(mufile, json);

        }
    }
}
