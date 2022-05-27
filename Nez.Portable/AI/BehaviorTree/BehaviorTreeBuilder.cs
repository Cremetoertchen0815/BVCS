using System;
using System.Collections.Generic;


namespace Nez.AI.BehaviorTrees
{
    /// <summary>
    /// helper for building a BehaviorTree using a fluent API. Leaf nodes need to first have a parent added. Parents can be Composites or
    /// Decorators. Decorators are automatically closed when a leaf node is added. Composites must have endComposite called to close them.
    /// </summary>
    public class BehaviorTreeBuilder<T>
    {
        private T _context;

        /// <summary>
        /// Last node created.
        /// </summary>
        private Behavior<T> _currentNode;

        /// <summary>
        /// Stack nodes that we are build via the fluent API.
        /// </summary>
        private Stack<Behavior<T>> _parentNodeStack = new Stack<Behavior<T>>();


        public BehaviorTreeBuilder(T context) => _context = context;


        public static BehaviorTreeBuilder<T> Begin(T context) => new BehaviorTreeBuilder<T>(context);

        private BehaviorTreeBuilder<T> SetChildOnParent(Behavior<T> child)
        {
            var parent = _parentNodeStack.Peek();
            if (parent is Composite<T>)
            {
                (parent as Composite<T>).AddChild(child);
            }
            else if (parent is Decorator<T>)
            {
                // Decorators have just one child so end it automatically
                (parent as Decorator<T>).Child = child;
                EndDecorator();
            }

            return this;
        }


        /// <summary>
        /// pushes a Composite or Decorator on the stack
        /// </summary>
        /// <returns>The parent node.</returns>
        /// <param name="composite">Composite.</param>
        private BehaviorTreeBuilder<T> PushParentNode(Behavior<T> composite)
        {
            if (_parentNodeStack.Count > 0)
                SetChildOnParent(composite);

            _parentNodeStack.Push(composite);
            return this;
        }

        private BehaviorTreeBuilder<T> EndDecorator()
        {
            _currentNode = _parentNodeStack.Pop();
            return this;
        }


        #region Leaf Nodes (actions and sub trees)

        public BehaviorTreeBuilder<T> Action(Func<T, TaskStatus> func)
        {
            Insist.IsFalse(_parentNodeStack.Count == 0,
                "Can't create an unnested Action node. It must be a leaf node.");
            return SetChildOnParent(new ExecuteAction<T>(func));
        }


        /// <summary>
        /// Like an action node but the function can return true/false and is mapped to success/failure.
        /// </summary>
        public BehaviorTreeBuilder<T> Action(Func<T, bool> func) => Action(t => func(t) ? TaskStatus.Success : TaskStatus.Failure);


        public BehaviorTreeBuilder<T> Conditional(Func<T, TaskStatus> func)
        {
            Insist.IsFalse(_parentNodeStack.Count == 0,
                "Can't create an unnested Conditional node. It must be a leaf node.");
            return SetChildOnParent(new ExecuteActionConditional<T>(func));
        }


        /// <summary>
        /// Like a conditional node but the function can return true/false and is mapped to success/failure.
        /// </summary>
        public BehaviorTreeBuilder<T> Conditional(Func<T, bool> func) => Conditional(t => func(t) ? TaskStatus.Success : TaskStatus.Failure);


        public BehaviorTreeBuilder<T> LogAction(string text)
        {
            Insist.IsFalse(_parentNodeStack.Count == 0,
                "Can't create an unnested Action node. It must be a leaf node.");
            return SetChildOnParent(new LogAction<T>(text));
        }


        public BehaviorTreeBuilder<T> WaitAction(float waitTime)
        {
            Insist.IsFalse(_parentNodeStack.Count == 0,
                "Can't create an unnested Action node. It must be a leaf node.");
            return SetChildOnParent(new WaitAction<T>(waitTime));
        }


        /// <summary>
        /// Splice a sub tree into the parent tree.
        /// </summary>
        public BehaviorTreeBuilder<T> SubTree(BehaviorTree<T> subTree)
        {
            Insist.IsFalse(_parentNodeStack.Count == 0,
                "Can't splice an unnested sub tree, there must be a parent tree.");
            return SetChildOnParent(new BehaviorTreeReference<T>(subTree));
        }

        #endregion


        #region Decorators

        public BehaviorTreeBuilder<T> ConditionalDecorator(Func<T, TaskStatus> func, bool shouldReevaluate = true)
        {
            var conditional = new ExecuteActionConditional<T>(func);
            return PushParentNode(new ConditionalDecorator<T>(conditional, shouldReevaluate));
        }


        /// <summary>
        /// Like a conditional decorator node but the function can return true/false and is mapped to success/failure.
        /// </summary>
        public BehaviorTreeBuilder<T> ConditionalDecorator(Func<T, bool> func, bool shouldReevaluate = true) => ConditionalDecorator(t => func(t) ? TaskStatus.Success : TaskStatus.Failure, shouldReevaluate);


        public BehaviorTreeBuilder<T> AlwaysFail() => PushParentNode(new AlwaysFail<T>());


        public BehaviorTreeBuilder<T> AlwaysSucceed() => PushParentNode(new AlwaysSucceed<T>());


        public BehaviorTreeBuilder<T> Inverter() => PushParentNode(new Inverter<T>());


        public BehaviorTreeBuilder<T> Repeater(int count) => PushParentNode(new Repeater<T>(count));


        public BehaviorTreeBuilder<T> UntilFail() => PushParentNode(new UntilFail<T>());


        public BehaviorTreeBuilder<T> UntilSuccess() => PushParentNode(new UntilSuccess<T>());

        #endregion


        #region Composites

        public BehaviorTreeBuilder<T> Parallel() => PushParentNode(new Parallel<T>());


        public BehaviorTreeBuilder<T> ParallelSelector() => PushParentNode(new ParallelSelector<T>());


        public BehaviorTreeBuilder<T> Selector(AbortTypes abortType = AbortTypes.None) => PushParentNode(new Selector<T>(abortType));


        public BehaviorTreeBuilder<T> RandomSelector() => PushParentNode(new RandomSelector<T>());


        public BehaviorTreeBuilder<T> Sequence(AbortTypes abortType = AbortTypes.None) => PushParentNode(new Sequence<T>(abortType));


        public BehaviorTreeBuilder<T> RandomSequence() => PushParentNode(new RandomSequence<T>());


        public BehaviorTreeBuilder<T> EndComposite()
        {
            Insist.IsTrue(_parentNodeStack.Peek() is Composite<T>,
                "attempting to end a composite but the top node is a decorator");
            _currentNode = _parentNodeStack.Pop();
            return this;
        }

        #endregion


        public BehaviorTree<T> Build(float updatePeriod = 0.2f)
        {
            Insist.IsNotNull(_currentNode, "Can't create a behaviour tree with zero nodes");
            return new BehaviorTree<T>(_context, _currentNode, updatePeriod);
        }
    }
}