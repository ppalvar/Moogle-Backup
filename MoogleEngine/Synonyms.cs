namespace Synonyms;

using System;
using System.Text.Json;
using Settings;
using Utils;

public static class Synonyms{
    //diccionario que contiene como claves las palabras y como valor un array de strings con sus sinonimos
    private static Dictionary<string, string[]> synonyms = new Dictionary<string, string[]>();
    
    public static void Load(){
        //leer el archivo de sinonimos
        List <JsonItem>? items = JsonFileReader.Read <List <JsonItem>>(FileSettings.synonymsDir);
        if (items is null)return;

        //para cada par Clave -> Valor del diccionario de sinonimos, añadirlo a synonyms
        foreach (JsonItem item in items){
            string key = item.Key.ToLower();
            Tools.RemoveSpanishChars(ref key);
            if (!synonyms.ContainsKey(key))synonyms.Add(key, item.Value);
            else{
                synonyms[key] =  synonyms[key].Concat(item.Value).ToArray();
            } 
        }
    }

    public static string[] GetSynonyms(string word){
        if (synonyms.ContainsKey(word)){
            string[] syns = new string[synonyms[word].Length];//los sinonimos de la palabra
            for (int i = 0; i < synonyms[word].Length; i++){//adaptar las palabras para que sean lowercase y sin caracteres extraños
                syns[i] = synonyms[word][i].ToLower();
                Tools.RemoveSpanishChars(ref syns[i]);
            }
            return syns;
        }
        return new string[]{};//si no tiene sinonimos retornar un array vacio
    }
}

//lector generico para archivos '.json' que recibe un tipo T y devuelve un objeto de dicho tipo
//con el contenido del archivo
internal static class JsonFileReader
{
    public static T? Read<T>(string filePath)
    {
        string text = File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<T>(text);
    }
}

//clase para leer cada item del archivo '.json' de sinonimos
internal class JsonItem{
    public string Key{get;set;}
    public string[] Value{get;set;}

    public JsonItem(string Key, string[] Value){
        this.Key = Key;
        this.Value = Value;
    }
}