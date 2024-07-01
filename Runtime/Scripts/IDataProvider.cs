namespace HHG.UI.Runtime
{
    public interface IDataProvider
    {
        object GetDataWeak(object data);
    }

    public interface IDataProvider<T> : IDataProvider
    {
        T GetData(T data);
    }
}