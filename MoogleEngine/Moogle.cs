namespace MoogleEngine;

using FileHandlers;

public static class Moogle
{
    //esta funcion se llama antes de iniciar la aplicacion web para cargar
    //todos los archivos antes de que se haga la primera query
    public static void Start(){
        FileLoader.Load();
    }
    
    public static SearchResult Query(string query) {
        return FileLoader.Search(query.ToLower());
    }
}
