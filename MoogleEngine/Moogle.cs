namespace MoogleEngine;

using FileHandlers;

public static class Moogle
{
    //esta funcion se llama antes de iniciar la aplicacion web para cargar
    //todos los archivos antes de que se haga la primera query
    public static void Start(){
        FileLoader.Load();
    }

    //Este metodo retorna un snippet relativamente largo para un documento dado
    public static string Detail(string docName){
        return FileLoader.GetDetail(docName);
    }
    
    public static SearchResult Query(string query) {
        return FileLoader.Search(query.ToLower());
    }
}
