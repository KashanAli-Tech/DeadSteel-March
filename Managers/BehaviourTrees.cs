using System;
using System.Collections.Generic;

namespace Coursework.Managers
{
    // node results let the tree decide whether to continue, stop or try another branch
    // running is available in the tree types, but the current ai actions finish in one tick
    internal enum NodeStatus
    {
        Success,
        Failure,
        Running
    }

    // a shared tick method lets the tree combine different checks and actions through one interface
    internal interface IBtNode
    {
        NodeStatus Tick();
    }

    // checks children in order and stops when one does not succeed, like an and condition
    internal class SequenceNode : IBtNode
    {
        private readonly List<IBtNode> children;
        public SequenceNode(params IBtNode[] children) => this.children = new List<IBtNode>(children);

        // running later steps only after earlier ones succeed keeps actions behind their conditions
        public NodeStatus Tick()
        {
            foreach (var child in children)
            {
                var result = child.Tick();
                if (result != NodeStatus.Success)
                    return result;
            }
            return NodeStatus.Success;
        }
    }

    // tries children in order until one does not fail, like trying alternative choices
    internal class SelectorNode : IBtNode
    {
        private readonly List<IBtNode> children;
        public SelectorNode(params IBtNode[] children) => this.children = new List<IBtNode>(children);

        // returning the first non-failure lets this node choose a successful or still-running branch
        public NodeStatus Tick()
        {
            foreach (var child in children)
            {
                var result = child.Tick();
                if (result != NodeStatus.Failure)
                    return result;
            }
            return NodeStatus.Failure;
        }
    }

    // ticks every child in the same update so spawning, targeting and difficulty do not block each other
    // the children still run one after another here, not on separate threads
    internal class ParallelNode : IBtNode
    {
        private readonly List<IBtNode> children;
        public ParallelNode(params IBtNode[] children) => this.children = new List<IBtNode>(children);

        // reporting success when any child succeeds still allows every child to be checked
        public NodeStatus Tick()
        {
            bool anySuccess = false;
            foreach (var child in children)
            {
                if (child.Tick() == NodeStatus.Success)
                    anySuccess = true;
            }
            return anySuccess ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    // wrapping a true or false function lets timer and condition checks fit into the tree
    internal class ConditionNode : IBtNode
    {
        private readonly Func<bool> condition;
        public ConditionNode(Func<bool> condition) => this.condition = condition;
        public NodeStatus Tick() => condition() ? NodeStatus.Success : NodeStatus.Failure;
    }

    // wrapping an action lets the tree call an ai task after its condition passes
    internal class ActionNode : IBtNode
    {
        private readonly Action action;
        public ActionNode(Action action) => this.action = action;
        // these actions report success after they run because they do not return a separate failure result
        public NodeStatus Tick()
        {
            action();
            return NodeStatus.Success;
        }
    }
}