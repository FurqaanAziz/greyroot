using System;

namespace CardGame
{
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Notify(Card card, CardEvent cardEvent);
    }
}
