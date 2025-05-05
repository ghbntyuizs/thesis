using System.Diagnostics;

namespace SmartStorePOS.ViewModels.States
{
    /// <summary>
    /// Quản lý trạng thái của OrderViewModel
    /// </summary>
    public class OrderStateManager
    {
        private readonly OrderViewModel _context;
        private IOrderState _currentState;

        public OrderStateManager(OrderViewModel context)
        {
            _context = context;
            // Trạng thái mặc định là InitialState
            _currentState = new InitialState();
            _currentState.Enter(_context);
        }

        /// <summary>
        /// Chuyển đổi sang trạng thái mới
        /// </summary>
        /// <param name="newState">Trạng thái mới</param>
        public void TransitionTo(IOrderState newState)
        {
            Debug.WriteLine($"Chuyển trạng thái từ {_currentState.GetStateName()} sang {newState.GetStateName()}");
            _currentState = newState;
            _currentState.Enter(_context);
        }

        /// <summary>
        /// Lấy trạng thái hiện tại
        /// </summary>
        public IOrderState GetCurrentState()
        {
            return _currentState;
        }
    }
}
