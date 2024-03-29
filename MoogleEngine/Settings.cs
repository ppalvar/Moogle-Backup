namespace Settings;

/*
    This file contains configurations for the project as follows:

    [FileSettings]
    |
    |-(splitChars) -> a char array with chars to split the input documents
    |
    |-(baseDir)    -> the directory where is stored all data
    |
    |-(docsDir)    -> the directory where input documents are stored
    |
    |-(files)      -> a string array with all file names
    |
    |-(synonymsDir)-> the name of the synonyms '.json' file
*/

public static class FileSettings{ 
    public static char[] splitChars = "\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !\"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~\x7f\x80\x81\x82\x83\x84\x85\x86\x87\x88\x89\x8a\x8b\x8c\x8d\x8e\x8f\x90\x91\x92\x93\x94\x95\x96\x97\x98\x99\x9a\x9b\x9c\x9d\x9e\x9f\xa0¡¢£¤¥¦§¨©«¬\xad®¯°±´¶·¸»¿×÷".ToCharArray(); 

    public static string baseDir = "../";
    public static string docsDir = baseDir + "Content/";
    public static string[] files = Directory.GetFiles(docsDir, "*.txt");
    public static string synonymsDir = docsDir + "sinonimos.json";
}
