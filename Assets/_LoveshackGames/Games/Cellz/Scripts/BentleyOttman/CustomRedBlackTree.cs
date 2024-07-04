using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum COLOR { RED, BLACK }

public class CustomNode
{
    public line val;
    public COLOR color;
    public CustomNode left, right, parent;

    public CustomNode(line val)
    {
        this.val = val;
        parent = left = right = null;
        color = COLOR.RED;
    }

    public CustomNode Uncle()
    {
        if (parent == null || parent.parent == null)
            return null;

        if (parent.IsOnLeft())
            return parent.parent.right;
        else
            return parent.parent.left;
    }

    public bool IsOnLeft() { return this == parent.left; }

    public CustomNode Sibling()
    {
        if (parent == null)
            return null;

        if (IsOnLeft())
            return parent.right;

        return parent.left;
    }

    public void MoveDown(CustomNode nParent)
    {
        if (parent != null)
        {
            if (IsOnLeft())
                parent.left = nParent;
            else
                parent.right = nParent;
        }
        nParent.parent = parent;
        parent = nParent;
    }

    public bool HasRedChild()
    {
        return (left != null && left.color == COLOR.RED)
            || (right != null && right.color == COLOR.RED);
    }
}

public class CustomRedBlackTree
{
    CustomNode root;

    public void Print()
    {
        Debug.Log(PrintHelper(root));
    }

    string PrintHelper(CustomNode location)
    {
        if(location == null)
            return "";
        string toPrint = PrintHelper(location.left);
        if(location.left != null)
            toPrint += "\n"+location.val.id+" L: "+location.left.val.id;
        if(location.right != null){
            toPrint += " R: "+location.right.val.id+"\n";
        }
        toPrint += PrintHelper(location.right);
        return toPrint;
    }

    void LeftRotate(CustomNode x)
    {
        CustomNode nParent = x.right;

        if (x == root)
            root = nParent;

        x.MoveDown(nParent);

        x.right = nParent.left;

        if (nParent.left != null)
            nParent.left.parent = x;

        nParent.left = x;
    }

    void RightRotate(CustomNode x)
    {
        CustomNode nParent = x.left;

        if (x == root)
            root = nParent;

        x.MoveDown(nParent);

        x.left = nParent.right;

        if (nParent.right != null)
            nParent.right.parent = x;

        nParent.right = x;
    }

    void SwapColors(CustomNode x1, CustomNode x2)
    {
        COLOR temp = x1.color;
        x1.color = x2.color;
        x2.color = temp;
    }

    void SwapValues(CustomNode u, CustomNode v)
    {
        line temp = u.val;
        u.val = v.val;
        v.val = temp;
    }

    void FixRedRed(CustomNode x)
    {
        if (x == root)
        {
            x.color = COLOR.BLACK;
            return;
        }

        CustomNode parent = x.parent, grandparent = parent.parent,
             uncle = x.Uncle();

        if (parent.color != COLOR.BLACK)
        {
            if (uncle != null && uncle.color == COLOR.RED)
            {
                parent.color = COLOR.BLACK;
                uncle.color = COLOR.BLACK;
                grandparent.color = COLOR.RED;
                FixRedRed(grandparent);
            }
            else
            {
                if (parent.IsOnLeft())
                {
                    if (x.IsOnLeft())
                    {
                        SwapColors(parent, grandparent);
                    }
                    else
                    {
                        LeftRotate(parent);
                        SwapColors(x, grandparent);
                    }
                    RightRotate(grandparent);
                }
                else
                {
                    if (x.IsOnLeft())
                    {
                        RightRotate(parent);
                        SwapColors(x, grandparent);
                    }
                    else
                    {
                        SwapColors(parent, grandparent);
                    }
                    LeftRotate(grandparent);
                }
            }
        }
    }

    CustomNode Successor(CustomNode x)
    {
        CustomNode temp = x;
        while (temp.left != null)
            temp = temp.left;
        return temp;
    }

    CustomNode BSTreplace(CustomNode x)
    {
        if (x.left != null && x.right != null)
            return Successor(x.right);

        if (x.left == null && x.right == null)
            return null;

        return x.left != null ? x.left : x.right;
    }

    void DeleteNode(CustomNode v)
    {
        CustomNode u = BSTreplace(v);

        bool uvBlack = ((u == null || u.color == COLOR.BLACK) && (v.color == COLOR.BLACK));
        CustomNode parent = v.parent;

        if (u == null)
        {
            if (v == root)
            {
                root = null;
            }
            else
            {
                if (uvBlack)
                {
                    FixDoubleBlack(v);
                }
                else
                {
                    if (v.Sibling() != null)
                        v.Sibling().color = COLOR.RED;
                }

                if (v.IsOnLeft())
                {
                    parent.left = null;
                }
                else
                {
                    parent.right = null;
                }
            }
            return;
        }

        if (v.left == null || v.right == null)
        {
            if (v == root)
            {
                v.val = u.val;
                v.left = v.right = null;
                DeleteNode(u);
            }
            else
            {
                if (v.IsOnLeft())
                {
                    parent.left = u;
                }
                else
                {
                    parent.right = u;
                }
                u.parent = parent;

                if (uvBlack)
                {
                    FixDoubleBlack(u);
                }
                else
                {
                    u.color = COLOR.BLACK;
                }
            }
            return;
        }

        SwapValues(u, v);
        DeleteNode(u);
    }

    void FixDoubleBlack(CustomNode x)
    {
        if (x == root)
            return;

        CustomNode sibling = x.Sibling(), parent = x.parent;

        if (sibling == null)
        {
            FixDoubleBlack(parent);
        }
        else
        {
            if (sibling.color == COLOR.RED)
            {
                parent.color = COLOR.RED;
                sibling.color = COLOR.BLACK;
                if (sibling.IsOnLeft())
                {
                    RightRotate(parent);
                }
                else
                {
                    LeftRotate(parent);
                }
                FixDoubleBlack(x);
            }
            else
            {
                if (sibling.HasRedChild())
                {
                    if (sibling.left != null && sibling.left.color == COLOR.RED)
                    {
                        if (sibling.IsOnLeft())
                        {
                            sibling.left.color = sibling.color;
                            sibling.color = parent.color;
                            RightRotate(parent);
                        }
                        else
                        {
                            sibling.left.color = parent.color;
                            RightRotate(sibling);
                            LeftRotate(parent);
                        }
                    }
                    else
                    {
                        if (sibling.IsOnLeft())
                        {
                            sibling.right.color = parent.color;
                            LeftRotate(sibling);
                            RightRotate(parent);
                        }
                        else
                        {
                            sibling.right.color = sibling.color;
                            sibling.color = parent.color;
                            LeftRotate(parent);
                        }
                    }
                    parent.color = COLOR.BLACK;
                }
                else
                {
                    sibling.color = COLOR.RED;
                    if (parent.color == COLOR.BLACK)
                        FixDoubleBlack(parent);
                    else
                        parent.color = COLOR.BLACK;
                }
            }
        }
    }

    public CustomNode GetRoot() { return root; }

    public CustomNode Search(line n, float x)
    {
        CustomNode temp = root;
        while (temp != null)
        {
            int comp = temp.val.CompareToAtX(n, x);
            if (comp > 0)
            {
                if (temp.left == null)
                    break;
                else
                    temp = temp.left;
            }
            else if (comp == 0)
            {
                break;
            }
            else
            {
                if (temp.right == null)
                    break;
                else
                    temp = temp.right;
            }
        }

        return temp;
    }

    public void Insert(line n, float x)
    {
        CustomNode newNode = new CustomNode(n);
        if (root == null)
        {
            newNode.color = COLOR.BLACK;
            root = newNode;
        }
        else
        {
            CustomNode temp = Search(n, x);

            if (temp.val.CompareToAtX(n, x) == 0)
            {
                return;
            }

            newNode.parent = temp;

            if (temp.val.CompareToAtX(n, x) > 0)
                temp.left = newNode;
            else
                temp.right = newNode;

            FixRedRed(newNode);
        }
    }

    public void DeleteByVal(line n, float x)
    {
        if (root == null)
            return;

        CustomNode v = Search(n, x);
        if (v.val.CompareToAtX(n, x) != 0)
        {
            Debug.Log("No node found to delete with value:" + n);
            return;
        }

        DeleteNode(v);
    }   

    public CustomNode Higher(line value, float x)
    {
        CustomNode current = root;
        CustomNode successor = null;

        while (current != null)
        {
            if (current.val.CompareToAtX(value, x) > 0)
            {
                successor = current;
                current = current.left;
            }
            else
            {
                current = current.right;
            }
        }

        return successor;
    }

    public CustomNode Lower(line value, float x)
    {
        CustomNode current = root;
        CustomNode predecessor = null;

        while (current != null)
        {
            if (current.val.CompareToAtX(value, x) < 0)
            {
                predecessor = current;
                current = current.right;
            }
            else
            {
                current = current.left;
            }
        }

        return predecessor;
    }
}
