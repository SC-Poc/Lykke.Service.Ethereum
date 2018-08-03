using System;
using System.Collections;
using System.Collections.Generic;

namespace Lykke.Service.EthereumWorker.Core.IntervalTree
{
    public partial class IntervalTree<TKey, TValue>
    {
        private sealed class Enumerator : IEnumerator<IIntervalValuePair<TKey, TValue>>
        {
            private readonly Node _root;
            
            private Action _action;
            private Node _current;
            private Node _right;
            
            
            public Enumerator(Node root)
            {
                _right = root;
                _root = root;
                _action = _root == null ? Action.End : Action.Right;
            }

            
            public IIntervalValuePair<TKey, TValue> Current => 
                _current;

            object IEnumerator.Current 
                => Current;
            
            
            public void Dispose()
            {
                
            }
            
            public bool MoveNext()
            {
                switch (_action)
                {
                    case Action.Right:
                        
                        _current = _right;

                        while (_current.Left != null)
                        {
                            _current = _current.Left;
                        }

                        _right = _current.Right;
                        _action = _right != null ? Action.Right : Action.Parent;

                        return true;
                    
                    
                    case Action.Parent:
                        
                        while (_current.Parent != null)
                        {
                            var previous = _current;

                            _current = _current.Parent;

                            if (_current.Left == previous)
                            {
                                _right = _current.Right;
                                _action = _right != null ? Action.Right : Action.Parent;

                                return true;
                            }
                        }

                        _action = Action.End;

                        return false;
                    
                    
                    case Action.End:
                        return false;
                    
                    
                    default:       
                        throw new ArgumentOutOfRangeException(nameof(_action), _action.ToString());
                }
            }

            public void Reset()
            {
                _right = _root;
                _action = _root == null ? Action.End : Action.Right;
            }

            private enum Action
            {
                Parent,
                Right,
                End
            }
        }
    }
}
