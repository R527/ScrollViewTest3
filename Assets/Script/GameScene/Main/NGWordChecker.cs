public static class NGWordChecker
{
    public static bool CheckNGWords(string preName,string[] ngList) {
        bool answerWord = true;
        foreach (string checkWord in ngList) {
            int num = preName.IndexOf(checkWord);
            if(num >= 0) {
                return answerWord = false;
            }
        }
        return answerWord;
    }
}
