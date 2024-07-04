using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections.Generic;

public class Node<T> where T : IComparable<T>
{
    public T val;
    public COLOR color;
    public Node<T> left, right, parent;

    public Node(T val)
    {
        this.val = val;
        parent = left = right = null;
        color = COLOR.RED;
    }

    public Node<T> Uncle()
    {
        if (parent == null || parent.parent == null)
            return null;

        if (parent.IsOnLeft())
            return parent.parent.right;
        else
            return parent.parent.left;
    }

    public bool IsOnLeft() { return this == parent.left; }

    public Node<T> Sibling()
    {
        if (parent == null)
            return null;

        if (IsOnLeft())
            return parent.right;

        return parent.left;
    }

    public void MoveDown(Node<T> nParent)
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

public class RBTree<T> where T : IComparable<T>
{
    Node<T> root;

    void LeftRotate(Node<T> x)
    {
        Node<T> nParent = x.right;

        if (x == root)
            root = nParent;

        x.MoveDown(nParent);

        x.right = nParent.left;

        if (nParent.left != null)
            nParent.left.parent = x;

        nParent.left = x;
    }

    void RightRotate(Node<T> x)
    {
        Node<T> nParent = x.left;

        if (x == root)
            root = nParent;

        x.MoveDown(nParent);

        x.left = nParent.right;

        if (nParent.right != null)
            nParent.right.parent = x;

        nParent.right = x;
    }

    void SwapColors(Node<T> x1, Node<T> x2)
    {
        COLOR temp = x1.color;
        x1.color = x2.color;
        x2.color = temp;
    }

    void SwapValues(Node<T> u, Node<T> v)
    {
        T temp = u.val;
        u.val = v.val;
        v.val = temp;
    }

    void FixRedRed(Node<T> x)
    {
        if (x == root)
        {
            x.color = COLOR.BLACK;
            return;
        }

        Node<T> parent = x.parent, grandparent = parent.parent,
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

    Node<T> Successor(Node<T> x)
    {
        Node<T> temp = x;
        while (temp.left != null)
            temp = temp.left;
        return temp;
    }

    Node<T> BSTreplace(Node<T> x)
    {
        if (x.left != null && x.right != null)
            return Successor(x.right);

        if (x.left == null && x.right == null)
            return null;

        return x.left != null ? x.left : x.right;
    }

    void DeleteNode(Node<T> v)
    {
        Node<T> u = BSTreplace(v);

        bool uvBlack = ((u == null || u.color == COLOR.BLACK) && (v.color == COLOR.BLACK));
        Node<T> parent = v.parent;

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

    void FixDoubleBlack(Node<T> x)
    {
        if (x == root)
            return;

        Node<T> sibling = x.Sibling(), parent = x.parent;

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

    public Node<T> GetRoot() { return root; }

    Node<T> Search(T n)
    {
        Node<T> temp = root;
        while (temp != null)
        {
            int comp = n.CompareTo(temp.val);
            if (comp < 0)
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

    public void Insert(T n)
    {
        Node<T> newNode = new Node<T>(n);
        if (root == null)
        {
            newNode.color = COLOR.BLACK;
            root = newNode;
        }
        else
        {
            Node<T> temp = Search(n);

            if (temp.val.CompareTo(n) == 0)
            {
                return;
            }

            newNode.parent = temp;

            if (n.CompareTo(temp.val) < 0)
                temp.left = newNode;
            else
                temp.right = newNode;

            FixRedRed(newNode);
        }
    }

    public void DeleteByVal(T n)
    {
        if (root == null)
            return;

        Node<T> v = Search(n);
        if (v.val.CompareTo(n) != 0)
        {
            Debug.Log("No node found to delete with value:" + n);
            return;
        }

        DeleteNode(v);
    }   

    public Node<T> Higher(T value)
    {
        Node<T> current = root;
        Node<T> successor = null;

        while (current != null)
        {
            if (current.val.CompareTo(value) > 0)
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

    public Node<T> Lower(T value)
    {
        Node<T> current = root;
        Node<T> predecessor = null;

        while (current != null)
        {
            if (current.val.CompareTo(value) < 0)
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