namespace CardGame
{
    public interface IObserver
    {
      void OnNotify(Card card, CardEvent cardEvent);
    }
}