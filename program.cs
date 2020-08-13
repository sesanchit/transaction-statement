using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Location
{
    public int id
    {
        get;
        set;
    }

    public string address
    {
        get;
        set;
    }

    public string city
    {
        get;
        set;
    }

    public int zipCode
    {
        get;
        set;
    }
}

public class TransactionData
{
    public int id
    {
        get;
        set;
    }

    public int userId
    {
        get;
        set;
    }

    public string userName
    {
        get;
        set;
    }

    public object timestamp
    {
        get;
        set;
    }

    public string txnType
    {
        get;
        set;
    }

    public string amount
    {
        get;
        set;
    }

    public Location location
    {
        get;
        set;
    }

    public string ip
    {
        get;
        set;
    }
}

public class Transaction
{
    public string page
    {
        get;
        set;
    }

    public int per_page
    {
        get;
        set;
    }

    public int total
    {
        get;
        set;
    }

    public int total_pages
    {
        get;
        set;
    }

    public IList<TransactionData> data
    {
        get;
        set;
    }
}

class Result
{
    /*
     * Complete the 'totalTransactions' function below.
     *
     * The function is expected to return a 2D_INTEGER_ARRAY.
     * The function accepts following parameters:
     *  1. INTEGER locationId
     *  2. STRING transactionType
     */
    public static List<List<int>> totalTransactions(int locationId, string transactionType)
    {
        List<List<int>> list = new List<List<int>>();
        
        int currentPage = 1;
        int totalPages = 0;

        IList<TransactionData> data = TransactionsAPI.apiResult(currentPage, transactionType, out totalPages);
        
        if (data.Count() > 0)
        {
            for (int i = 2; i <= totalPages; i++)
            {
                IList<TransactionData> newData = TransactionsAPI.apiResult(i, transactionType, out totalPages);
                data = data.Concat(newData).ToList();
            }
        }

        if (data.Count() > 0)
        {
            Dictionary<int, decimal> d = new Dictionary<int, decimal>();
            for (int i = 0; i < data.Count(); i++)
            {
                if (data[i].location.id == locationId)
                {
                    if (d.ContainsKey(data[i].userId))
                    {
                        decimal amountValue = decimal.Parse(data[i].amount, NumberStyles.Currency);
                        d[data[i].userId] = d[data[i].userId] + amountValue;
                    }
                    else
                    {
                        decimal amountValue = decimal.Parse(data[i].amount, NumberStyles.Currency);
                        d.Add(data[i].userId, amountValue);
                    }
                }
            }

            var keyList = d.Keys.ToList();
            keyList.Sort();
            foreach (var key in keyList)
            {
                List<int> contentInnerList = new List<int>();
                contentInnerList.Add(key);
                contentInnerList.Add((int)d[key]);
                list.Add(contentInnerList);
            }
        }

        if (list.Count() == 0){
            List<int> defaultInnerList = new List<int>();
            defaultInnerList.Add(-1);
            defaultInnerList.Add(-1);
            list.Add(defaultInnerList);
        }
        return list;
    }
}

class TransactionsAPI
{
    public static IList<TransactionData> apiResult(int pageNumber, string transactionType, out int total_pages)
    {
        HttpClient httpClient = new HttpClient();
        var response = httpClient.GetAsync($"https://jsonmock.hackerrank.com/api/transactions/search?txnType={transactionType}&page={pageNumber}");
        var result = response.Result.Content.ReadAsStringAsync().Result;
        Transaction jsonResult = JsonConvert.DeserializeObject<Transaction>(result);
        total_pages = jsonResult.total_pages;
        IList<TransactionData> data = jsonResult.data;
        return data;
    }
}

class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        int locationId = Convert.ToInt32(Console.ReadLine().Trim());

        string transactionType = Console.ReadLine();

        List<List<int>> result = Result.totalTransactions(locationId, transactionType);

        textWriter.WriteLine(String.Join("\n", result.Select(x => String.Join(" ", x))));

        textWriter.Flush();
        textWriter.Close();
    }
}
