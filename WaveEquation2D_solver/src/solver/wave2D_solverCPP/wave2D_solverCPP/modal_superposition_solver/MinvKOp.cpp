#include "MinvKOp.h"

MinvKOp::MinvKOp(const Eigen::SparseMatrix<double>& K_,
    Eigen::SimplicialLLT<Eigen::SparseMatrix<double>>& chol_)
    : m_K(K_), m_chol(chol_)
{
    // Verify the solver is initialized
    if (m_chol.info() != Eigen::Success)
    {
        // Handle error - maybe throw an exception
    }
}



void MinvKOp::perform_op(const double* x_in, double* y_out) const
{
    // Map input and output to Eigen vectors
    Eigen::Map<const Eigen::VectorXd> x(x_in, m_K.cols());
    Eigen::Map<Eigen::VectorXd> y(y_out, m_K.rows());

    // Compute K * x
    Eigen::VectorXd Kx = m_K * x;

    // Solve M * y = Kx  (i.e., y = M^{-1} * Kx)
    y = m_chol.solve(Kx);
}





