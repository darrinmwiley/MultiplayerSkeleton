using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BentleyOttmanLineIntersection : MonoBehaviour
{
    float epsilon = .001f;
    //modified bentley ottman:
    //given a sorted list of points (we can insertion sort it beginning each frame, benchmark and maybe move to nlogn but
    //insertion is O(inversions) so may be faster)

    //and a list of lines (pt1, pt2, line ID, open / closed, cclId) lineID will be negative for rays

    //insert these points into the prio heap in increasing X order

    //returns the right particles of ray intersections

    //idea: run this in update instead of fixed update, and just report intersecting circles best effort
    //fixedUpdate will just take what is calculated and act on it

    //assume the general form, we can correct for issues with this later

    public void Start(){
        List<line> lines = new List<line>(){
            new line(){
                p1 = new Vector2(1,3),
                p2 = new Vector2(3,3),
                id = 0
            },
            new line(){
                p1 = new Vector2(1,3),
                p2 = new Vector2(1,1),
                id = 1
            },
            new line(){
                p1 = new Vector2(1,1),
                p2 = new Vector2(3,1),
                id = 2
            },
            new line(){
                p1 = new Vector2(3,3),
                p2 = new Vector2(3,1),
                id = 3
            },
            new line(){
                p1 = new Vector2(0,1),
                p2 = new Vector2(4,3),
                id = 4
            },
        };
        GetIntersectionsHelper(lines);
    }

    public void GetIntersectionsHelper(List<line> lines)
    {
        foreach(line l in lines)
        {
            l.p1 = l.p1 + new Vector2(Random(), Random()) * epsilon;
            l.p2 = l.p2 + new Vector2(Random(), Random()) * epsilon;
            Debug.Log(l);
        }
        GetIntersections(lines);
    }

    public float Random(){
        return UnityEngine.Random.Range(0f,1f);
    }

    public void GetIntersections(List<line> lines)
    {
        PriorityQueue<Event> events = new PriorityQueue<Event>();
        foreach(line ln in lines){
            events.Add(new Event(){
                time = ln.p1.x,
                open = true,
                crossing = false,
                pt = ln.p1,
                lineId = ln.id,
            });
            events.Add(new Event(){
                time = ln.p2.x,
                open = false,
                lineId = ln.id,
                crossing = false,
                pt = ln.p2,
            });
        }
        CustomRedBlackTree rb = new CustomRedBlackTree();
        while(events.sz > 0)
        {
            Event e = events.Poll();
            float x = e.time;
            line l = lines[e.lineId];
            if(e.crossing){
                Debug.Log("crossing "+e.lineId+" "+e.lineId2+" "+x);
                CustomNode lower = rb.Search(lines[e.lineId2], x - epsilon);
                CustomNode higher = rb.Search(lines[e.lineId], x - epsilon);
                line save = lower.val;
                lower.val = higher.val;
                higher.val = save;
                
                CustomNode nextLower = rb.Lower(lower.val, x + epsilon);
                CustomNode nextHigher = rb.Higher(higher.val, x + epsilon);
                Vector2 higherIntersection;
                if(nextHigher != null && LineUtil.IntersectLineSegments2D(higher.val.p1, higher.val.p2, nextHigher.val.p1, nextHigher.val.p2, out higherIntersection)){
                    events.Add(new Event(){
                        time = higherIntersection.x,
                        open = false,
                        crossing = true,
                        lineId = nextHigher.val.id,
                        lineId2 = higher.val.id
                    });
                }
                Vector2 lowerIntersection;
                if(nextLower != null && LineUtil.IntersectLineSegments2D(lower.val.p1, lower.val.p2, nextLower.val.p1, nextLower.val.p2, out lowerIntersection))
                {
                    events.Add(new Event(){
                        time = lowerIntersection.x,
                        open = false,
                        crossing = true,
                        lineId = lower.val.id,
                        lineId2 = nextLower.val.id,
                    });
                }
            }else if(e.open){
                Debug.Log("adding "+l+" "+x);
                rb.Insert(l,x);
                CustomNode higher = rb.Higher(l,x + epsilon);
                CustomNode lower = rb.Lower(l,x + epsilon);
                Vector2 higherIntersection;
                if(higher != null && LineUtil.IntersectLineSegments2D(l.p1, l.p2, higher.val.p1, higher.val.p2, out higherIntersection)){
                    events.Add(new Event(){
                        time = higherIntersection.x,
                        open = false,
                        crossing = true,
                        lineId = higher.val.id,
                        lineId2 = l.id
                    });
                }
                Vector2 lowerIntersection;
                if(lower != null && LineUtil.IntersectLineSegments2D(l.p1, l.p2, lower.val.p1, lower.val.p2, out lowerIntersection))
                {
                    events.Add(new Event(){
                        time = lowerIntersection.x,
                        open = false,
                        crossing = true,
                        lineId = l.id,
                        lineId2 = lower.val.id,
                    });
                }
            }else{
                Debug.Log("removing "+l+" "+x);
                CustomNode higher = rb.Higher(l,x + epsilon);
                CustomNode lower = rb.Lower(l,x + epsilon);
                Vector2 intersection;
                if(higher != null && lower != null && LineUtil.IntersectLineSegments2D(higher.val.p1, higher.val.p2, lower.val.p1, lower.val.p2, out intersection))
                {
                    events.Add(new Event(){
                        time = intersection.x,
                        open = false,
                        crossing = true,
                        lineId = higher.val.id,
                        lineId2 = lower.val.id,
                    });
                }
                rb.DeleteByVal(lines[e.lineId], e.time);
            }
            rb.Print();
        }
    }

    public class Event : IComparable<Event>
    {
        public float time;
        public bool open, crossing;
        public int lineId;

        public Vector2 pt;

        //lineId2 will be the lower line
        public int lineId2;

        // Implementing IComparable interface
        public int CompareTo(Event other)
        {
            // First, compare by time
            int timeComparison = time.CompareTo(other.time);

            // If times are equal, compare by open flag (false < true)
            if (timeComparison == 0)
            {
                return open.CompareTo(other.open);
            }
            else
            {
                return timeComparison; // Return the comparison result based on time
            }
        }
    }

}
