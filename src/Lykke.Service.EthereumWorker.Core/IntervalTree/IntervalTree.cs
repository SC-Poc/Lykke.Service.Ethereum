using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Lykke.Service.EthereumWorker.Core.IntervalTree
{
    [PublicAPI]
    public sealed partial class IntervalTree<TKey, TValue> : IEnumerable<IIntervalValuePair<TKey, TValue>>
        where TKey : struct, IComparable<TKey>
    {
        private Node _root;


        public void Add(IIntervalValuePair<TKey, TValue> intervalValue)
        {
            Add(intervalValue.Interval, intervalValue.Value);
        }
        
        public void Add(Interval<TKey> interval, TValue value)
        {
            if (_root == null)
            {
                _root = new Node
                {
                    Interval = interval,
                    Value = value
                };
            }
            else
            {
                var node = _root;

                while (node != null)
                {
                    var comparison = interval.CompareTo(node.Interval);

                    if (comparison < 0)
                    {
                        var left = node.Left;

                        if (left == null)
                        {
                            node.Left = new Node
                            {
                                Interval = interval,
                                Value = value,
                                Parent = node
                            };

                            BalanceOnInsert(node, 1);

                            return;
                        }
                        else
                        {
                            node = left;
                        }
                    }
                    else if (comparison > 0)
                    {
                        var right = node.Right;

                        if (right == null)
                        {
                            node.Right = new Node
                            {
                                Interval = interval,
                                Value = value,
                                Parent = node
                            };

                            BalanceOnInsert(node, -1);

                            return;
                        }
                        else
                        {
                            node = right;
                        }
                    }
                    else
                    {
                        node.Value = value;

                        return;
                    }
                }
            }
        }

        public void AddRange(IEnumerable<IIntervalValuePair<TKey, TValue>> intervalValues)
        {
            foreach (var intervalValue in intervalValues)
            {
                Add(intervalValue.Interval, intervalValue.Value);
            }
        }
        
        public void AddRange(IEnumerable<(Interval<TKey> Interval, TValue Value)> intervalValues)
        {
            foreach (var intervalValue in intervalValues)
            {
                Add(intervalValue.Interval, intervalValue.Value);
            }
        }
        
        public void Clear()
        {
            _root = null;
        }

        public bool Contains(TKey point)
        {
            return TryFind(point, out _);
        }
        
        public bool Contains(Interval<TKey> interval)
        {
            return TryFind(interval, out _);
        }

        public IIntervalValuePair<TKey, TValue> Find(TKey point)
        {
            TryFind(point, out var result);

            return result;
        }
        
        public TValue Find(Interval<TKey> interval)
        {
            TryFind(interval, out var result);

            return result;
        }
        
        public IEnumerator<IIntervalValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(_root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IIntervalValuePair<TKey, TValue> Last()
        {
            return Reverse().First();
        }
        
        public IIntervalValuePair<TKey, TValue> LastOrDefault()
        {
            return Reverse().FirstOrDefault();
        }
        
        public bool Remove(Interval<TKey> interval)
        {
            var node = _root;

            while (node != null)
            {
                if (interval.CompareTo(node.Interval) < 0)
                {
                    node = node.Left;
                }
                else if (interval.CompareTo(node.Interval) > 0)
                {
                    node = node.Right;
                }
                else
                {
                    var left = node.Left;
                    var right = node.Right;

                    if (left == null)
                    {
                        if (right == null)
                        {
                            if (node == _root)
                            {
                                _root = null;
                            }
                            else
                            {
                                var parent = node.Parent;

                                if (parent.Left == node)
                                {
                                    parent.Left = null;

                                    BalanceOnDelete(parent, -1);
                                }
                                else
                                {
                                    parent.Right = null;

                                    BalanceOnDelete(parent, 1);
                                }
                            }
                        }
                        else
                        {
                            Replace(node, right);

                            BalanceOnDelete(node, 0);
                        }
                    }
                    else if (right == null)
                    {
                        Replace(node, left);

                        BalanceOnDelete(node, 0);
                    }
                    else
                    {
                        var successor = right;

                        if (successor.Left == null)
                        {
                            var parent = node.Parent;

                            successor.Parent = parent;
                            successor.Left = left;
                            successor.Balance = node.Balance;

                            left.Parent = successor;

                            if (node == _root)
                            {
                                _root = successor;
                            }
                            else
                            {
                                if (parent.Left == node)
                                {
                                    parent.Left = successor;
                                }
                                else
                                {
                                    parent.Right = successor;
                                }
                            }

                            BalanceOnDelete(successor, 1);
                        }
                        else
                        {
                            while (successor.Left != null)
                            {
                                successor = successor.Left;
                            }

                            var parent = node.Parent;
                            var successorParent = successor.Parent;
                            var successorRight = successor.Right;

                            if (successorParent.Left == successor)
                            {
                                successorParent.Left = successorRight;
                            }
                            else
                            {
                                successorParent.Right = successorRight;
                            }

                            if (successorRight != null)
                            {
                                successorRight.Parent = successorParent;
                            }

                            successor.Parent = parent;
                            successor.Left = left;
                            successor.Balance = node.Balance;
                            successor.Right = right;
                            right.Parent = successor;

                            left.Parent = successor;

                            if (node == _root)
                            {
                                _root = successor;
                            }
                            else
                            {
                                if (parent.Left == node)
                                {
                                    parent.Left = successor;
                                }
                                else
                                {
                                    parent.Right = successor;
                                }
                            }

                            BalanceOnDelete(successorParent, -1);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public IEnumerable<IIntervalValuePair<TKey, TValue>> Reverse()
        {
            // TODO: Replace with efficient algorithm
            
            return ((IEnumerable<IIntervalValuePair<TKey, TValue>>) this).Reverse();
        }

        public bool TryFind(TKey point, out IIntervalValuePair<TKey, TValue> intervalValue)
        {
            var node = _root;

            while (node != null)
            {
                if (node.Interval.CompareTo(point) > 0)
                {
                    node = node.Left;
                }
                else if (node.Interval.CompareTo(point) < 0)
                {
                    node = node.Right;
                }
                else
                {
                    intervalValue = node;

                    return true;
                }
            }

            intervalValue = null;

            return false;
        }
        
        public bool TryFind(Interval<TKey> interval, out TValue value)
        {
            var node = _root;

            while (node != null)
            {
                if (node.Interval.CompareTo(interval) > 0)
                {
                    node = node.Left;
                }
                else if (node.Interval.CompareTo(interval) < 0)
                {
                    node = node.Right;
                }
                else
                {
                    value = node.Value;

                    return true;
                }
            }

            value = default(TValue);

            return false;
        }

        
        #region Balancing
        
        private void BalanceOnDelete(Node node, int balance)
        {
            while (node != null)
            {
                balance = (node.Balance += balance);

                if (balance == 2)
                {
                    if (node.Left.Balance >= 0)
                    {
                        node = RotateRight(node);

                        if (node.Balance == -1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = RotateLeftRight(node);
                    }
                }
                else if (balance == -2)
                {
                    if (node.Right.Balance <= 0)
                    {
                        node = RotateLeft(node);

                        if (node.Balance == 1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = RotateRightLeft(node);
                    }
                }
                else if (balance != 0)
                {
                    return;
                }

                var parent = node.Parent;

                if (parent != null)
                {
                    balance = parent.Left == node ? -1 : 1;
                }

                node = parent;
            }
        }
        
        private void BalanceOnInsert(Node node, int balance)
        {
            while (node != null)
            {
                balance = (node.Balance += balance);

                if (balance == 0)
                {
                    return;
                }
                else if (balance == 2)
                {
                    if (node.Left.Balance == 1)
                    {
                        RotateRight(node);
                    }
                    else
                    {
                        RotateLeftRight(node);
                    }

                    return;
                }
                else if (balance == -2)
                {
                    if (node.Right.Balance == -1)
                    {
                        RotateLeft(node);
                    }
                    else
                    {
                        RotateRightLeft(node);
                    }

                    return;
                }

                var parent = node.Parent;

                if (parent != null)
                {
                    balance = parent.Left == node ? 1 : -1;
                }

                node = parent;
            }
        }
        
        #endregion
        
        #region Rotations
        
        private Node RotateLeft(Node node)
        {
            var right = node.Right;
            var rightLeft = right.Left;
            var parent = node.Parent;

            right.Parent = parent;
            right.Left = node;
            node.Right = rightLeft;
            node.Parent = right;

            if (rightLeft != null)
            {
                rightLeft.Parent = node;
            }

            if (node == _root)
            {
                _root = right;
            }
            else if (parent.Right == node)
            {
                parent.Right = right;
            }
            else
            {
                parent.Left = right;
            }

            right.Balance++;
            
            node.Balance = -right.Balance;

            return right;
        }
        
        private Node RotateRight(Node node)
        {
            var left = node.Left;
            var leftRight = left.Right;
            var parent = node.Parent;

            left.Parent = parent;
            left.Right = node;
            node.Left = leftRight;
            node.Parent = left;

            if (leftRight != null)
            {
                leftRight.Parent = node;
            }

            if (node == _root)
            {
                _root = left;
            }
            else if (parent.Left == node)
            {
                parent.Left = left;
            }
            else
            {
                parent.Right = left;
            }

            left.Balance--;
            
            node.Balance = -left.Balance;

            return left;
        }
        
        private Node RotateLeftRight(Node node)
        {
            var left = node.Left;
            var leftRight = left.Right;
            var parent = node.Parent;
            var leftRightRight = leftRight.Right;
            var leftRightLeft = leftRight.Left;

            leftRight.Parent = parent;
            node.Left = leftRightRight;
            left.Right = leftRightLeft;
            leftRight.Left = left;
            leftRight.Right = node;
            left.Parent = leftRight;
            node.Parent = leftRight;

            if (leftRightRight != null)
            {
                leftRightRight.Parent = node;
            }

            if (leftRightLeft != null)
            {
                leftRightLeft.Parent = left;
            }

            if (node == _root)
            {
                _root = leftRight;
            }
            else if (parent.Left == node)
            {
                parent.Left = leftRight;
            }
            else
            {
                parent.Right = leftRight;
            }

            if (leftRight.Balance == -1)
            {
                node.Balance = 0;
                left.Balance = 1;
            }
            else if (leftRight.Balance == 0)
            {
                node.Balance = 0;
                left.Balance = 0;
            }
            else
            {
                node.Balance = -1;
                left.Balance = 0;
            }

            leftRight.Balance = 0;

            return leftRight;
        }
        
        private Node RotateRightLeft(Node node)
        {
            var right = node.Right;
            var rightLeft = right.Left;
            var parent = node.Parent;
            var rightLeftLeft = rightLeft.Left;
            var rightLeftRight = rightLeft.Right;

            rightLeft.Parent = parent;
            node.Right = rightLeftLeft;
            right.Left = rightLeftRight;
            rightLeft.Right = right;
            rightLeft.Left = node;
            right.Parent = rightLeft;
            node.Parent = rightLeft;

            if (rightLeftLeft != null)
            {
                rightLeftLeft.Parent = node;
            }

            if (rightLeftRight != null)
            {
                rightLeftRight.Parent = right;
            }

            if (node == _root)
            {
                _root = rightLeft;
            }
            else if (parent.Right == node)
            {
                parent.Right = rightLeft;
            }
            else
            {
                parent.Left = rightLeft;
            }

            if (rightLeft.Balance == 1)
            {
                node.Balance = 0;
                right.Balance = -1;
            }
            else if (rightLeft.Balance == 0)
            {
                node.Balance = 0;
                right.Balance = 0;
            }
            else
            {
                node.Balance = 1;
                right.Balance = 0;
            }

            rightLeft.Balance = 0;

            return rightLeft;
        }
        
        #endregion

        #region Replacement

        private static void Replace(Node target, Node source)
        {
            var left = source.Left;
            var right = source.Right;

            target.Balance = source.Balance;
            target.Interval = source.Interval;
            target.Value = source.Value;
            target.Left = left;
            target.Right = right;

            if (left != null)
            {
                left.Parent = target;
            }

            if (right != null)
            {
                right.Parent = target;
            }
        }

        #endregion
    }
}
