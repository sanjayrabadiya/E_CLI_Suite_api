namespace GSC.Respository.PropertyMapping
{
    public interface ITypeHelperService
    {
        bool TypeHasProperties<T>(string fields);
    }
}