namespace FileHandlers;

using Utils;
using System;
using Settings;

internal class FileManager{
    /*
    clase encargada de gestionar cada documento independientemente
    encargandose de calcular su tf-idf,  distancia vectorial con la query,
    obtener el snippet y obtener el nombre del documento
    */
    private Dictionary <string, double> tf_idf = new Dictionary<string, double>();
    public string DocName{get;private set;}
    public string NameAsTitle{//obtener el nombre del documento en un formato presentable al usuario
        get{
            string[] tmp = this.DocName.Split("/");
            string  _tmp = String.Join(" ", tmp[tmp.Length - 1].Split(".")[0].Split("_")).ToUpper();
            return _tmp;
        }
    }

    //cantidad de veces que se repite cada palabra en este documento
    private Dictionary <string, int> wordCounter = new Dictionary<string, int>();
    private int totalWords;//cantidad de palabras en el documento
    private string content;

    //catidad de documentos en los que aparece una palabra dada
    private static Dictionary<string, int> docCorpusWordCounter = new Dictionary<string, int>();
    private static int totalDocCount = 0;//total de documentos

    public static string[] GetAllWords(){
        string[] words = docCorpusWordCounter.Keys.ToArray();

        return words;
    }

    public FileManager(string docName){//constructor de la clase
        this.DocName = docName;
        this.content = File.ReadAllText(docName);totalDocCount++;

        string contentCopy = this.content.ToLower();Tools.RemoveSpanishChars(ref contentCopy);

        string[] docWords = contentCopy.Split(FileSettings.splitChars, StringSplitOptions.RemoveEmptyEntries);

        
        ConvertToVector(ref docWords);
    }

    public void Normalize(){//calcula el idf de cada palabra y la multiplica por su valor de tf guardado
        foreach (var p in this.wordCounter){
            double idf = Math.Log((double) totalDocCount / (double) docCorpusWordCounter[p.Key] + 1.0);
            //le sumo 1 para evitar divisiones por cero
            tf_idf[p.Key] *= idf;
        }
    }

    public double GetVectorialDistance(ref string[] queryItems, ref string _query){
        //calcula el coseno entre el vector de la query y el vector del documento

        double x = 0;
        double y = 0;
        double z = 0;

        Dictionary <string, double> queryTf_idf = QueryAsVector(ref queryItems);//el vector query

        bool shouldHide = false;
        string lastWord = "";

        foreach (var p in queryTf_idf){
            //si una palabra no aparece en el documento y esta marcada como que debe aparecer
            //entonces se marca el documento como oculto y sera ignorado
            if (!this.tf_idf.ContainsKey(p.Key)){
                shouldHide = shouldHide || Tools.ShowOperator(ref _query, p.Key);
                continue;
            }

            x += p.Value * this.tf_idf[p.Key];
            y += p.Value * p.Value;
            z += this.tf_idf[p.Key] * this.tf_idf[p.Key];

            //multiplicar el valor de la palabra por la 2 elevado a la cantidad de '*' que tiene mas uno
            //asi si no tiene ninguno su valor se mantiene
            int importanceMultiplier = Tools.ImportanceOperator(ref _query, p.Key);x *= (1 << importanceMultiplier);
            
            //si una palabra aparece en el documento y esta marcada como que NO debe aparecer
            //entonces se marca el documento como oculto y sera ignorado
            shouldHide = shouldHide || Tools.HideOperator(ref _query, p.Key);

            //multiplicar las palabras marcadas con '~' por 1/x, con x = cantidad minima de caracteres
            //entre dos ocurrencias de las palabras en el documento
            double distanceMultiplier = Tools.DistanceOperator(ref _query, ref lastWord, p.Key, ref this.content);

            x /= distanceMultiplier;
            
            lastWord = p.Key;
        }

        //si la palabra esta marcada como oculta se retorna valor 0
        //sino el valor de la palabra va ser el resultado de la formula del coseno entre dos vectores
        return shouldHide? 0.0 : x / (Math.Sqrt(y) * Math.Sqrt(z) + 1.0);
    }

    public string GetSnippet(ref string[] query){
        #region SearchBestSnippet
        int SnippetLength = 500;

        //indices donde aparece la primera letra de las palabras de la query en este documento
        List <int> indexes = Tools.GetAppearingIndexes(ref this.content, ref query);

        indexes.Sort();

        //indices que indican un rango de la lista indexes donde se buscara la mayor
        //cantidad de palabras que coincidan con la query
        int leftPointer = 0;
        int rightPointer = 0;
        
        //indices que guardaran el mejor segmento del documento teniendo como criterio
        //que contenga la mayor cantidad de palabras presentes en la query sin ver su importancia semantica
        int leftMaxPointer = 0;
        int rightMaxPointer = 0;

        //maxima cantidad de palabras en comun con la query contenida en un intervalo
        int max = int.MinValue;

        foreach (int i in indexes){
            //mover el indice de la izquierda todo lo necesario
            while (indexes[rightPointer] - indexes[leftPointer] + 1 > SnippetLength){
                leftPointer++;
            }
            if (!(indexes[rightPointer] - indexes[leftPointer] + 1 > SnippetLength) && max < rightPointer - leftPointer){
            //si la longitud del intervalo actual es menor que la longitud fimaxima fijada para un 
            //snippet y la cantidad de palabras en comun con la query es mayor que max,
            //entonces se actualizan las variables correspondientes
                leftMaxPointer = leftPointer;
                rightMaxPointer = rightPointer;
                max = indexes[rightPointer] - indexes[leftPointer];
            }
            rightPointer ++;//mover el indice de la derecha una posicion hacia la derecha
        }
        #endregion

        #region BuildSnippet
        
        string snippet = "";//el snippet que debe ser devuelto

        //marcador para las palabras en comun con la query
        //true  -> se esta añadiendo una palabra relacionada con la query
        //false -> no se esta añadiendo una palabra relacionada con la query
        bool addingQueriedWord = false;
        
        for (int i = Math.Min(indexes[leftMaxPointer], this.content.Length) - 1; i < Math.Min(Math.Max(indexes[rightMaxPointer], indexes[leftMaxPointer] + SnippetLength - 1), this.content.Length); i++){
            if (indexes.Contains(i)){
                addingQueriedWord = true;//marcar la palabra como "añadiendo"
                snippet += "<mark>";//si la palabra esta en la query añadir una etiqueta <mark> para resaltarla
                SnippetLength += 6;//sumar la longitud de la etiqueta <mark> a la longitud del snippet para que se mantenga constante
            }
            else if (addingQueriedWord && !Tools.IsAlphanum(this.content[i - 1])){
                snippet += "</mark>";//cerrar la etiqueta <mark>
                addingQueriedWord = false;//desmarcar la palabra como "añadiendo"
                SnippetLength += 7;//sumar la longitud de la etiqueta </mark> a la longitud del snippet para que se mantenga constante
            }
            if (i - 1 >= 0)snippet += this.content[i - 1];//añadir el un cararter al snippet
        }
        if (snippet.Length < SnippetLength){//si el snippet es muy pequeño añadir mas caracteres
            for (int i = indexes[leftMaxPointer] - 2; i - 1 >= 0 && snippet.Length < SnippetLength; i--){
                snippet = this.content[i - 1] + snippet;
            }
        }
        #endregion

        return snippet;
    }

    private Dictionary<string, double> QueryAsVector(ref string[] query){
        //en esata funcion se calcula el tf-idf para la query con un proceso analogo al usado para los documentos
        
        Dictionary <string, int> wordCounter = new Dictionary<string, int>();
        
        int totalWords = 0;
        
        foreach (string word in query){
            if (!wordCounter.ContainsKey(word))wordCounter.Add(word, 0);
            wordCounter[word] ++;totalWords++;
        }

        Dictionary <string, double> queryTf_idf = new Dictionary<string, double>();

        foreach (var p in wordCounter){
            int amountInDocs = 0;
            if (docCorpusWordCounter.ContainsKey(p.Key))amountInDocs = docCorpusWordCounter[p.Key];

            double tf = ((double) p.Value + 1) / ((double) totalWords + 1);
            double idf = Math.Log(( (double) totalDocCount + 1.0) / ((double) amountInDocs + 1.0));

            queryTf_idf.Add(p.Key, tf * idf);
        }

        return queryTf_idf;
    }

    private void ConvertToVector(ref string[] document){
        totalWords = 0;
        foreach (string word in document){
            if (!this.wordCounter.ContainsKey(word)){//primera vez que aparece una palabra en el documento
                this.wordCounter.Add(word, 0);//añadir la palabra al diccionario del documento
                
                if (!docCorpusWordCounter.ContainsKey(word))docCorpusWordCounter.Add(word, 0);
                
                docCorpusWordCounter[word]++;//sumar uno a la cantidad de documentos donde aparece la palabra
            }
            this.wordCounter[word]++;
            this.totalWords ++;
        }

        foreach (var p in wordCounter){//ahora solo se calcula el tf, el idf se calcula mas tarde con el metodo Normalize
            this.tf_idf[p.Key] = ((double) this.wordCounter[p.Key]) / ((double) this.totalWords + 1);
            if (p.Key == "japon")Console.WriteLine(tf_idf[p.Key]);
            //los 1 sumados son para equilibrar la formula y para evitar divisiones por cero respectivamente
        }
    }

    public static int GetWordFrequency(string word){
        if (!docCorpusWordCounter.ContainsKey(word))return 0;//si la palabra no aparece su fecuencia es 0
        return docCorpusWordCounter[word];
    }
}
