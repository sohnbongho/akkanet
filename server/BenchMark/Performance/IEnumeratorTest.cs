using BenchmarkDotNet.Attributes;
using System.Collections.Concurrent;

namespace BenchMark.Performance;

[MemoryDiagnoser]
public class IEnumeratorTest
{
    [Benchmark]
    public string ArrayTest()
    {
        string[] arr = new string[10];
        for (int i = 0; i < 10; i++)
        {
            arr[i] = i.ToString();
        }

        string result = "";
        for (int i = 0; i < arr.Length; i++)
        {
            result += arr[i];
        }

        return result;
    }
    [Benchmark]
    public string ListTest()
    {
        List<string> arr = new List<string>(10);
        for (int i = 0; i < 10; i++)
        {
            arr.Add(i.ToString());
        }

        string result = "";
        for (int i = 0; i < arr.Count; i++)
        {
            result += arr[i];
        }

        return result;
    }

    [Benchmark]
    public string NormalListTest()
    {
        List<string> arr = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            arr.Add(i.ToString());
        }

        string result = "";
        for (int i = 0; i < arr.Count; i++)
        {
            result += arr[i];
        }

        return result;
    }

    [Benchmark]
    public string DictionaryTest()
    {
        Dictionary<int, string> arr = new Dictionary<int, string>(10);
        for (int i = 0; i < 10; i++)
        {
            //arr.Add(i, i.ToString());
            arr[i] = i.ToString();
        }

        string result = "";
        for (int i = 0; i < arr.Count; i++)
        {
            result += arr[i];
        }

        return result;
    }

    [Benchmark]
    public string NormalDictionaryWithLockTest()
    {
        var lockObj = new Object();
        Dictionary<int, string> arr = new Dictionary<int, string>();
        for (int i = 0; i < 10; i++)
        {
            lock (lockObj)
            {
                arr[i] = i.ToString();
            }   
        }

        string result = "";
        for (int i = 0; i < arr.Count; i++)
        {
            lock (lockObj)
            {
                result += arr[i];
            }                
        }

        return result;
    }

    [Benchmark]
    public string NormalDictionaryTest()
    {
        Dictionary<int, string> arr = new Dictionary<int, string>();
        for (int i = 0; i < 10; i++)
        {
            //arr.Add(i, i.ToString());
            arr[i] = i.ToString();
        }

        string result = "";
        for (int i = 0; i < arr.Count; i++)
        {
            result += arr[i];
        }

        return result;
    }
    [Benchmark]
    public string NormalDictionaryWithTryCatchTest()
    {
        try
        {
            Dictionary<int, string> arr = new Dictionary<int, string>();
            for (int i = 0; i < 10; i++)
            {
                //arr.Add(i, i.ToString());
                arr[i] = i.ToString();
            }

            string result = "";
            for (int i = 0; i < arr.Count; i++)
            {
                result += arr[i];
            }

            return result;
        }
        catch
        {
            return string.Empty;
        }        
    }
    [Benchmark]
    public string ConcurrenctDictionaryTest()
    {
        ConcurrentDictionary<int, string> arr = new ConcurrentDictionary<int, string>();
        for (int i = 0; i < 10; i++)
        {
            //arr.Add(i, i.ToString());
            arr[i] = i.ToString();
        }

        string result = "";
        for (int i = 0; i < arr.Count; i++)
        {
            result += arr[i];
        }

        return result;
    }

    [Benchmark]
    public string NormalDictionaryWitchReadWriteLockTest()
    {
        ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();        
        Dictionary<int, string> arr = new Dictionary<int, string>();

        readWriteLock.EnterWriteLock();
        for (int i = 0; i < 10; i++)
        {
            //arr.Add(i, i.ToString());
            arr[i] = i.ToString();
        }
        readWriteLock.ExitWriteLock();
                
        string result = "";
        readWriteLock.EnterReadLock();
        for (int i = 0; i < arr.Count; i++)
        {
            result += arr[i];
        }
        readWriteLock.ExitReadLock();

        return result;
    }
}
