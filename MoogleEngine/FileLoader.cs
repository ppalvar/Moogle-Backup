namespace FileHandlers;

using Utils;
using System;
using Settings;
using MoogleEngine;
using Synonyms;

public static class FileLoader{
    private static bool isInitialized = false;//marca los documentos como cargados para evitar leerlos de nuevo

    private static FileManager[] allFiles = new FileManager[0];//instancias de FileManager que se encargan de TODO en los documentos

    public static void Load(){
        if (isInitialized)return;
        
        allFiles = new FileManager[FileSettings.files.Length];
        
        int i = 0;//variable para recorrer los documentos por su indice
        foreach (string filename in FileSettings.files){
            Console.Write($"Loading document {filename}.....");
            
            allFiles[i++] = new FileManager(filename);//añadir los archivos instanciandolos en la clase FileManager

            Console.WriteLine("DONE");
        }

        for (i = 0; i < allFiles.Length; i++){
            allFiles[i].Normalize();//normalizar los documentos para calcular su tf-idf
        }

        Suggester.Initialize();//cargar las palabras en la clase Suggester para hacer sugerencias al usuario
        Synonyms.Load();//cargar los sinonimos para la busqueda semantica
        
        isInitialized = true;
    }

    public static SearchResult Search(string query){
        Tools.RemoveSpanishChars(ref query);

        //los terminos de la busqueda mas sus sinonimos (si aparecen marcados para busqueda semantica)
        string[] searchTerms = Tools.GetQueryAndSynonyms(ref query);
        
        //los terminos originales de la busqueda sin sinonimos
        string[] originalsSearchTerms = query.Split(FileSettings.splitChars, StringSplitOptions.RemoveEmptyEntries);
        
        //inicializar un Thread que haga las sugerencias a la vez que la busqueda
        Suggester.query = originalsSearchTerms;
        Thread suggestionsThread = new Thread(Suggester.MakeSuggestions);
        suggestionsThread.Start();

        //lista de resultados (lista para añadirlos de forma dinamica)
        List <SearchItem> tmp = new List<SearchItem>();

        //valor que fija la minima importancia que debe tener un documento para ser tenido en cuenta
        const double epsylon = 0.001;

        foreach (FileManager file in allFiles){
            double value = file.GetVectorialDistance(ref searchTerms, ref query);
            if (value > epsylon){
                tmp.Add(new SearchItem(file.NameAsTitle, file.GetSnippet(ref searchTerms), (float) value));
            }
        }

        tmp.Sort();

        SearchItem[] result = tmp.ToArray();//convertir el resultado a un array para retornarlo

        suggestionsThread.Join();
        string[] _suggestion = Suggester.Suggestion;

        //para cada palabra sugerida primero comprobar si es distinta a su correspondiente en la query
        //luego si AL MENOS 1 palabra de las sugeridas NO esta en la query entonces se muestra dicha sugerencia
        //en caso contrario se deja la sujerencia como un string vacio
        bool flag = false;
        for (int i = 0; i < originalsSearchTerms.Length; i++){
            string tmp1 = originalsSearchTerms[i];Tools.RemoveSpanishChars(ref tmp1);
            string tmp2 = _suggestion[i];Tools.RemoveSpanishChars(ref tmp2);
            
            flag = flag || tmp1 != tmp2;
        }

        string suggestion = String.Join(" ", _suggestion);

        if (!flag)suggestion = "";

        return new SearchResult(result, suggestion);
    }
}
