namespace Utils;

using Synonyms;
using Settings;

public static class Tools{
    public static bool IsAlphanum(char c){
        string chars = "áéíóúüabcdefghijklmnñopqrstuvwxyz0123456789";
        return chars.Contains(c.ToString().ToLower());
    }

    //metodo que retorna una lista de enteros con los indices donde aparece el primer caracter de todos
    //los terminos de la busqueda contenidos en el array query
    public static List<int> GetAppearingIndexes(ref string text, ref string[] query){
        List <int> indexes = new List<int>();
        for (int i = 0; i < text.Length; i++){
            string tmp = "";
            while (i < text.Length && IsAlphanum(text[i])){//añadir caracteres alfanumericos contiguos...
                tmp += text[i++].ToString().ToLower();
            }

            RemoveSpanishChars(ref tmp);//eliminar caracteres extraños

            if (tmp != "" && query.Contains(tmp)){
                indexes.Add(i - tmp.Length + 1);//si esta en la query, lo añado a la lista           
            }
        }
        return indexes;
    }

    //metodo que cambia los caracteres de un string por equivalentes del alfabeto ingles
    public static void RemoveSpanishChars(ref string s){
        string[] sol = new string[s.Length];
        for (int i = 0; i < s.Length; i ++){
            switch (s[i]){
                case 'á':sol[i] = "a";break;
                case 'é':sol[i] = "e";break;
                case 'í':sol[i] = "i";break;
                case 'ó':sol[i] = "o";break;
                case 'ú':sol[i] = "u";break;
                case 'ü':sol[i] = "u";break;
                case 'ç':sol[i] = "c";break;
                default: sol[i] = s[i].ToString();break;
            }
        }s = String.Join("", sol);
    }
    
    //operadores obligatorios para la evaluacion del trabajo
    #region RequiredOperators
    /*
    todos los operadores funcionan de forma similar excepto el de distancia:
        1) se recorre la query dada como un string
        2) se toman caracteres alfanumericos contiguos que formen palabras
        3) se chequea que dichas palabras esten marcadas con algun operador
            3.1)si estan marcadas con '*' se retorna la cantidad de '*' mas uno
            3.2)si estan marcadas con '!' y coincide con el parametro word se retorna true, sino falso
            3.2)si estan marcadas con '^' y coincide con el parametro word se retorna true, sino falso
    
    el operador de distancia(~) funciona de la siguiente manera:
        1)se localizan todos los caracteres '~'
        2)se busca hacia la izquierda y hacia la derecha los conjuntos contiguos de caracteres
          alfanumericos(palabras) que coincidan con wordA y con wordB
        3)se retorna el logaritmo en base 2 de la minima cantidad de caracteres entre dos ocurrencias 
          de wordA y wordB con una llamada a DistanceCalculator
        4)en caso de que no exista al menos un caracter '~' se retorna 1, para que el valor de dicha palabra se mantenga
        5)si existe al menos un caracter '~' pero las palabras que modifica no existen en el documento
          se retorna un valor arbitrariamente grande, puesto que en ese caso se considera que estan a
          distancia infinita
    */
    public static int ImportanceOperator(ref string query, string word){//ImportanceOperator = '*' [*···*palabra]
        int max = 1;
        for (int i = 0; i < query.Length; i++){
            string tmp = "";
            while (i < query.Length && IsAlphanum(query[i])){
                tmp += query[i++].ToString().ToLower();
            }

            RemoveSpanishChars(ref tmp);

            if (tmp == word){
                int j = i - tmp.Length - 1;
                int tmpMax = 1;
                while (0 <= j && j < query.Length && query[j] == '*'){
                    tmpMax++;j--;
                }
                max = Math.Max(max, tmpMax);
            }
        }
        return max;
    }

    public static bool ShowOperator(ref string query, string word){//ShowOperator = '^' [^palabra]
        bool flag = false;
        for (int i = 0; i < query.Length; i++){
            string tmp = "";
            while (i < query.Length && IsAlphanum(query[i])){
                tmp += query[i++].ToString().ToLower();
            }

            RemoveSpanishChars(ref tmp);

            if (tmp == word){
                int j = i - tmp.Length - 1;
                if (0 <= j && j < query.Length && tmp == word && query[j] == '^'){
                    flag = true;break;
                }
            }
        }
        return flag;
    }

    public static bool HideOperator(ref string query, string word){//HideOperator = '!' [!palabra]
        bool flag = false;
        for (int i = 0; i < query.Length; i++){
            string tmp = "";
            while (i < query.Length && IsAlphanum(query[i])){
                tmp += query[i++].ToString().ToLower();
            }

            RemoveSpanishChars(ref tmp);

            if (tmp == word){
                int j = i - tmp.Length - 1;
                if (0 <= j && j < query.Length && tmp == word && query[j] == '!'){
                    flag = true;break;
                }
            }
        }
        return flag;
    }

    private static double DistanceCalculator(ref string content, ref string wordA, ref string wordB){
        List <int> a = new List<int>();
        List <int> b = new List<int>();

        List <int> wordASize = new List<int>();

        for (int i = 0; i < content.Length; i++){
            string tmp = "";
            while (i < content.Length && IsAlphanum(content[i])){
                tmp += content[i++].ToString().ToLower();
            }

            RemoveSpanishChars(ref tmp);

            if (tmp == wordA){
                int j = i - tmp.Length;
                a.Add(j);wordASize.Add(tmp.Length);
            }
            else if (tmp == wordB){
                int j = i - tmp.Length;
                b.Add(j);
            }
        }

        int min = 1000000000;//numero arbitrariamente grande para garantizar que cualquier valor con el que se compare, sera mas pequeño
        for (int i = 0, j = 0; i < a.Count && j < b.Count;){
            while (i < a.Count && j < b.Count && a[i] < b[j]){
                int tmpMin = b[j] - (a[i] + wordASize[i] - 1);
                min = Math.Min(min, tmpMin);i++;
            }j++;
        }
        return Math.Log2(min + 2);//sumar dos al logaritmo para que siempre sea al menos 1 y al multiplicar el valor de las palabras no disminuya en unos documentos sino que aumente en otros
    }

    public static double DistanceOperator(ref string query, ref string wordA, string wordB, ref string docContent){
        bool flag = false;

        for (int i = 0; i < query.Length; i++){
            if (query[i] == '~'){
                flag = true;

                int j = i, k = i;
                string tmpWordA = "";
                
                while (j >= 0 && !IsAlphanum(query[j]))j--;
                while (j >= 0 && IsAlphanum(query[j])) tmpWordA = query[j--].ToString().ToLower() + tmpWordA;

                RemoveSpanishChars(ref tmpWordA);

                string tmpWordB = "";

                while (k < query.Length && !IsAlphanum(query[k]))k++;
                while (k < query.Length && IsAlphanum(query[k])) tmpWordB += query[k++].ToString().ToLower();

                RemoveSpanishChars(ref tmpWordB);

                if (wordA == tmpWordA && wordB == tmpWordB){
                    return DistanceCalculator(ref docContent, ref wordA, ref wordB);
                }
            }
        }
        // 1 -> no hay operador '~' en la query
        // 1000000 -> las palabras que modifica el operador no existen en el documento(al menos una de las dos)
        return flag ? 1000000.0 : 1.0;
    }
    #endregion

    #region CustomOperators

    /*
    operadores custom:
        '?' -> operador semantico(para buscar una palabra tambien por sus sinonimos) [palabra?]
            1)primero se busca todas las ocurrencias del caracter '?'
            2)luego, la palabra que se encuantre anterior al operador se le buscan sus sinonimos
              llamando a la funcion Synonyms.GetSynonyms
            3)se concatenan todos los sinonimos de las palabras marcadas con '?' junto con los terminos originales
              de la query
            4)este array es el que sera usado para la busqueda y asi seran tenidos en cuenta todos los sinonimos
              de las palabras marcadas con '?'
    */

    public static string[] GetQueryAndSynonyms(ref string _query){
        string[] query = _query.Split(FileSettings.splitChars, StringSplitOptions.RemoveEmptyEntries);
        string[] allSynonyms = SemanticOperator(ref _query);

        query = query.Concat(allSynonyms).ToArray();
        return query;
    }

    private static string[] SemanticOperator(ref string query){//SemanticOperator = '?' [palabra?]
        bool isMarked = false;
        string[] allSynonyms = {};
        for (int i = query.Length - 1; i >= 0 ; i--){
            if (query[i] == '?')isMarked = true;
            else if (isMarked){
                string tmp = "";
                while (i >= 0 && !IsAlphanum(query[i]))i--;

                while (i >= 0 && IsAlphanum(query[i])){
                    tmp = query[i--].ToString().ToLower() + tmp;
                }

                RemoveSpanishChars(ref tmp);

                string[] wordSynonyms = Synonyms.GetSynonyms(tmp);
                allSynonyms = allSynonyms.Concat(wordSynonyms).ToArray();

                isMarked = false;
            }
        }
        return allSynonyms;
    }

    #endregion
}