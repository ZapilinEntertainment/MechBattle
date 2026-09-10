using System.Collections.Generic;
using UnityEngine;
using ZE.UiService;

namespace ZE.MechBattle
{
    public class CompositeWindowBinder : IWindowBinder
    {
        private readonly List<IWindowBinder> _binders = new();

        public CompositeWindowBinder(params IWindowBinder[] binders)
        {
            _binders = new(binders);
        }

        public void AddBinder(IWindowBinder binder) => _binders.Add(binder);

        public async Awaitable LoadWindow()
        {
            var bindersCount = _binders.Count;
            if (bindersCount == 0)
                return;

            if (bindersCount < 4)
            {
                for (var i = 0; i < bindersCount;i++)
                {
                    await _binders[i].LoadWindow();
                }
            }
            else
            {
                var awaitableList = new Awaitable[bindersCount];
                for (var i = 0; i < bindersCount; i++)
                {
                    awaitableList[i] = _binders[i].LoadWindow();
                }
                await AwaitablesExtensions.WhenAll(awaitableList);
            }
        }

        public void RegisterWindow(WindowsManager windowsManager)
        {
            var bindersCount = _binders.Count;
            if (bindersCount == 0)
                return;

            for (var i = 0; i< bindersCount; i++)
            {
                _binders[i].RegisterWindow(windowsManager);
            }
        }
    }
}
