namespace PrintNode
{
    public class R<T>
    {
        public int code {  get; set; }

        public string msg { get; set; }

        public T data { get; set; }
        public static R<T> success(T objects)
        {
            R<T> r = new R<T>();
            r.data = objects;
            r.code = 1;
            return r;
            
        }
        public static R<string> error(String msg)
        {
            R<string> r = new R<string>();
            r.msg = msg;
            r.code = 0;
            return r;
        }
    }
}
