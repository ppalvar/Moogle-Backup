namespace FileHandlers;

using System;

internal static class Suggester{
    public static string[] Suggestion = {};//aqui se guardara la sugerencia
    public static string[] query = {};//antes de llamar a MakeSuggestions, gurdar los terminos de la query aqui
    private static string[] words = {};//todas las palabras que existen en el cuerpo de documentos
    public static void Initialize(){
        words = FileManager.GetAllWords();
        int[] wordsLength = new int[words.Length];//las longitudes de las palabras

        for (int i = 0; i < words.Length; i++){
            wordsLength[i] = words[i].Length;
        }

        Array.Sort(wordsLength, words);//ordenar las palabras por longitud
    }
    
    public static void MakeSuggestions(){
        string[] result = new string[query.Length];

        int i = 0;
        foreach (string word in query){
            result[i++] = GetBestCoincidence(word);
        }

        Suggestion = result;
    }

    private static int GetMinEditDistance(string wordA, string wordB){
        //implementacion de minimum edit distance para calcular la minima diferencia entre palabras
        //ver https://en.wikipedia.org/wiki/Levenshtein_distance para mas info
        int[,] memo = new int[wordA.Length + 1, wordB.Length + 1];

        for (int j = 0; j <= wordB.Length; j++){
            for (int i = 0; i <= wordA.Length; i++){
                if (i == 0)memo[i, j] = j;
                else if (j == 0)memo[i, j] = i;
                else if (wordA[i - 1] == wordB[j - 1])memo[i, j] = memo[i - 1, j - 1];
                else memo[i, j] = 1 + Math.Min(memo[i, j - 1], Math.Min(memo[i - 1, j], memo[i - 1, j - 1]));
            }
        }
        return memo[wordA.Length, wordB.Length];
    }

    private static int GetWordIndex(string word){
        //buscar el minimo indice de una palabra con longitud mayor o igual a word
        //el algoritmo utilizado es busqueda binaria ya que las palabras estan ordenadas por longitud
        int left = 0, right = words.Length - 1;

        while (left < right){
            int middle = (left + right) / 2;

            if (words[middle].Length < word.Length)left = middle + 1;
            else right = middle;
        }
        return right;
    }

    private static string GetBestCoincidence(string word){
        int wordIndex = GetWordIndex(word);

        int minDiff = 1000;//no hay dos palabras a y b cuyo GetMinEditDistance(a, b) >= 1000
        int maxFreq = 0;   //la maxima frecuencia con que aparece una palabra con minima distancia a word
        string bestCoincidence = "";//la mejor coincidencia
        
        //buscar desde el indice indicado por la longitud de la palabra hacia la derecha
        //las siguientes 5000 palabras como margen en el que se espera encontrar alguna coincidencia
        //la idea es que palabras mucho mas largas no sean tenidas en cuenta
        for (int i = wordIndex; i < words.Length && (i - wordIndex < 5000 || words[i].Length <= word.Length); i++){
            int diff = GetMinEditDistance(word, words[i]);
            if (minDiff > diff || (minDiff == diff && FileManager.GetWordFrequency(words[i]) > maxFreq)){
                minDiff = diff;
                maxFreq = FileManager.GetWordFrequency(words[i]);
                bestCoincidence = words[i];
            }
        }

        //lo mismo que el ciclo for anterior pero revisando las 5000 palabras anteriores
        //con tal de que exista un margen para palabras mas cortas pero que no sea demasiado grande
        for (int i = wordIndex - 1; i >= 0 && wordIndex - i < 5000; i--){
            int diff = GetMinEditDistance(word, words[i]);
            if (minDiff > diff || (minDiff == diff && FileManager.GetWordFrequency(words[i]) > maxFreq)){
                minDiff = diff;
                maxFreq = FileManager.GetWordFrequency(words[i]);
                bestCoincidence = words[i];
            }
        }

        return bestCoincidence;
    }
}