using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PriorityQueue<T> where T : IComparable<T>
{
    public List<T> events = new List<T>();
    public int sz = 0;

    public void Add(T e)
    {
        events.Add(e);
        int index = sz;
        int p = (index - 1) / 2;
        while (index != 0 && events[p].CompareTo(e) > 0)
        {
            events[index] = events[p];
            index = p;
            p = (index - 1) / 2;
        }
        events[index] = e;
        sz++;
    }

    public T Poll()
    {
        T ret = events[0];
        sz--;
        if(sz == 0){
            return ret;
            events.RemoveAt(sz);
        }
        T last = events[sz];
        events.RemoveAt(sz);
        int index = 0;
        int childIndex = index * 2 + 1;
        if (childIndex + 1 < sz && events[childIndex].CompareTo(events[childIndex + 1]) > 0)
        {
            childIndex++;
        }
        while (childIndex < sz && events[childIndex].CompareTo(last) < 0)
        {
            events[index] = events[childIndex];
            index = childIndex;
            childIndex = index * 2 + 1;
            if (childIndex + 1 < sz && events[childIndex].CompareTo(events[childIndex + 1]) > 0)
            {
                childIndex++;
            }
        }
        events[index] = last;
        return ret;
    }
}